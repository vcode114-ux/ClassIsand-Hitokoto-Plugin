using CommunityToolkit.Mvvm.ComponentModel;

namespace HitokotoPlugin.Models;

/// <summary>
/// 插件全局设置（持久化到 Settings.json）
/// </summary>
public partial class PluginSettings : ObservableObject
{
    /// <summary>
    /// 是否启用豆包 AI 内容审核
    /// </summary>
    [ObservableProperty]
    private bool _enableAiReview = false;

    /// <summary>
    /// 豆包 AI 平台的 API Key
    /// </summary>
    [ObservableProperty]
    private string _doubaoApiKey = string.Empty;

    /// <summary>
    /// 豆包 AI 使用的模型 ID（在火山引擎控制台创建的推理接入点 ID）
    /// </summary>
    [ObservableProperty]
    private string _doubaoModelId = "ep-xxxxxxxxxx";  // 用户需替换为自己的接入点 ID

    /// <summary>
    /// 是否启用千问 AI（通义千问）内容审核
    /// </summary>
    [ObservableProperty]
    private bool _enableQwenReview = false;

    /// <summary>
    /// 千问 AI 的 API Key（阿里云 DashScope）
    /// </summary>
    [ObservableProperty]
    private string _qwenApiKey = string.Empty;

    /// <summary>
    /// 千问使用的模型 ID（DashScope 模型，如 qwen-turbo、qwen-plus 等）
    /// </summary>
    [ObservableProperty]
    private string _qwenModelId = "qwen-turbo";

    /// <summary>
    /// 一言 API 地址（支持自定义，留空则使用默认）
    /// </summary>
    [ObservableProperty]
    private string _hitokotoApiUrl = "https://v1.hitokoto.cn/?c=i";

    /// <summary>
    /// 当 AI 审核拒绝时，是否在主区域显示具体拒绝原因
    /// </summary>
    [ObservableProperty]
    private bool _showRejectionReason = false;
}
