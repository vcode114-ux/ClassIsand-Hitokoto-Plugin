using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Threading;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Attributes;
using HitokotoPlugin.Models;
using HitokotoPlugin.Services;

namespace HitokotoPlugin.Views.Components;

[ComponentInfo(
    "A3F1C2D4-88B7-4E9A-BC34-1F2E3A4B5C6D",   // ← 用 `dotnet new guid` 换成你自己的
    "一言",
    "\uE9B0",
    "每 20 秒展示一句来自 hitokoto.cn 的一言，支持豆包 AI 内容审核"
)]
public partial class HitokotoComponent : ComponentBase<HitokotoComponentSettings>
{
    // ── Avalonia StyledProperty 定义 ─────────────────────────────────────
    public static readonly StyledProperty<string> HitokotoProperty =
        AvaloniaProperty.Register<HitokotoComponent, string>(nameof(Hitokoto), "正在加载一言…");

    public static readonly StyledProperty<string> SourceProperty =
        AvaloniaProperty.Register<HitokotoComponent, string>(nameof(Source), string.Empty);

    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<HitokotoComponent, bool>(nameof(IsLoading), true);

    public static readonly StyledProperty<string> StatusMessageProperty =
        AvaloniaProperty.Register<HitokotoComponent, string>(nameof(StatusMessage), string.Empty);

    public static readonly StyledProperty<string> RejectionReasonProperty =
        AvaloniaProperty.Register<HitokotoComponent, string>(nameof(RejectionReason), string.Empty);

    public string Hitokoto
    {
        get => GetValue(HitokotoProperty);
        set => SetValue(HitokotoProperty, value);
    }

    public string Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public string StatusMessage
    {
        get => GetValue(StatusMessageProperty);
        set => SetValue(StatusMessageProperty, value);
    }

    public string RejectionReason
    {
        get => GetValue(RejectionReasonProperty);
        set => SetValue(RejectionReasonProperty, value);
    }

    // ── 私有字段 ─────────────────────────────────────────────────────────
    private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(10) };
    private CancellationTokenSource? _cts;

    // 通过构造注入获取插件全局设置（ClassIsland 会通过 DI 容器实例化组件）
    private readonly PluginSettings _pluginSettings;

    public HitokotoComponent(PluginSettings pluginSettings)
    {
        _pluginSettings = pluginSettings;
        InitializeComponent();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        StartRefreshLoop();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    private void StartRefreshLoop()
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                await FetchAndUpdateAsync();
                try { await Task.Delay(TimeSpan.FromSeconds(20), token); }
                catch (TaskCanceledException) { break; }
            }
        }, token);
    }

    private async Task FetchAndUpdateAsync()
    {
        await Dispatcher.UIThread.InvokeAsync(() => { IsLoading = true; StatusMessage = string.Empty; });

        HitokotoResponse? resp = null;
        try
        {
            var apiUrl = string.IsNullOrWhiteSpace(_pluginSettings.HitokotoApiUrl)
                ? "https://v1.hitokoto.cn/?c=i"
                : _pluginSettings.HitokotoApiUrl;

            var json = await _http.GetStringAsync(apiUrl);
            resp = JsonSerializer.Deserialize<HitokotoResponse>(json);
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Hitokoto = "一言获取失败";
                Source = string.Empty;
                StatusMessage = $"网络错误：{ex.Message}";
                IsLoading = false;
            });
            return;
        }

        if (resp is null)
        {
            await Dispatcher.UIThread.InvokeAsync(() => { Hitokoto = "响应解析失败"; IsLoading = false; });
            return;
        }

        // 豆包 AI 审核（可选）
        if (_pluginSettings.EnableAiReview)
        {
            var (passed, reason) = await DoubaoReviewService.ReviewAsync(
                _pluginSettings.DoubaoApiKey,
                _pluginSettings.DoubaoModelId,
                resp.Hitokoto, resp.From, resp.FromWho);

            if (!passed)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    RejectionReason = reason;
                    if (_pluginSettings.ShowRejectionReason)
                    {
                        Hitokoto = $"[内容不符要求] {reason}";
                        Source = string.Empty;
                        StatusMessage = "AI 审核（豆包）：此条被过滤";
                    }
                    else
                    {
                        Hitokoto = "（此条一言已被 AI 过滤）";
                        Source = string.Empty;
                        StatusMessage = $"AI 审核（豆包）：{reason}";
                    }
                    IsLoading = false;
                });
                return;
            }
        }

        // 千问 AI 审核（可选）
        if (_pluginSettings.EnableQwenReview)
        {
            var (passed, reason) = await QwenReviewService.ReviewAsync(
                _pluginSettings.QwenApiKey,
                _pluginSettings.QwenModelId,
                resp.Hitokoto, resp.From, resp.FromWho);

            if (!passed)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    RejectionReason = reason;
                    if (_pluginSettings.ShowRejectionReason)
                    {
                        Hitokoto = $"[内容不符要求] {reason}";
                        Source = string.Empty;
                        StatusMessage = "AI 审核（千问）：此条被过滤";
                    }
                    else
                    {
                        Hitokoto = "（此条一言已被 AI 过滤）";
                        Source = string.Empty;
                        StatusMessage = $"AI 审核（千问）：{reason}";
                    }
                    IsLoading = false;
                });
                return;
            }
        }

        var sourceText = string.IsNullOrWhiteSpace(resp.FromWho)
            ? $"——《{resp.From}》"
            : $"——{resp.FromWho}·《{resp.From}》";

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Hitokoto = resp.Hitokoto;
            Source = sourceText;
            StatusMessage = string.Empty;
            RejectionReason = string.Empty;
            IsLoading = false;
        });
    }
}
