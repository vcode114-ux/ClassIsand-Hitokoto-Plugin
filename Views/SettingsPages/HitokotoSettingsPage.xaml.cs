using Avalonia.Controls;
using Avalonia.Interactivity;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Enums.SettingsWindow;
using HitokotoPlugin.Models;

namespace HitokotoPlugin.Views.SettingsPages;

/// <summary>
/// 插件全局设置页面：配置豆包 AI 审核开关与 API Key。
/// </summary>
[SettingsPageInfo(
    "dev.hitokoto.plugin.settings",  // 设置页面唯一 ID
    "一言插件设置",                    // 显示名称
    SettingsPageCategory.External    // 显示在"扩展设置"分组
)]
public partial class HitokotoSettingsPage : SettingsPageBase
{
    public PluginSettings Settings { get; }

    public HitokotoSettingsPage(PluginSettings settings)
    {
        Settings = settings;
        InitializeComponent();
        DataContext = this;
    }

    private async void DoubaoReviewToggle_OnChecked(object? sender, RoutedEventArgs e)
    {
        var dialog = new Window
        {
            Title = "⚠️ 注意",
            Width = 420,
            Height = 200,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false
        };

        var panel = new StackPanel
        {
            Margin = new Avalonia.Thickness(20),
            Spacing = 16
        };

        panel.Children.Add(new TextBlock
        {
            Text = "此项没有进行测试，可能会出现 BUG，请谨慎选择或使用千问 AI。",
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            FontSize = 14
        });

        var buttonPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Spacing = 10
        };

        var cancelBtn = new Button { Content = "取消", Width = 80, Margin = new Avalonia.Thickness(0) };
        var confirmBtn = new Button { Content = "确认开启", Width = 80 };

        cancelBtn.Click += (_, _) =>
        {
            Settings.EnableAiReview = false;
            dialog.Close(false);
        };
        confirmBtn.Click += (_, _) => dialog.Close(true);

        panel.Children.Add(buttonPanel);
        buttonPanel.Children.Add(cancelBtn);
        buttonPanel.Children.Add(confirmBtn);

        dialog.Content = panel;

        var hostWindow = VisualRoot as Window;
        var result = await dialog.ShowDialog<bool?>(hostWindow);

        if (result != true)
        {
            Settings.EnableAiReview = false;
        }
        else
        {
            // 确认开启豆包 → 自动关闭千问
            Settings.EnableQwenReview = false;
        }
    }

    private void QwenReviewToggle_OnChecked(object? sender, RoutedEventArgs e)
    {
        // 开启千问 → 自动关闭豆包
        Settings.EnableAiReview = false;
    }
}
