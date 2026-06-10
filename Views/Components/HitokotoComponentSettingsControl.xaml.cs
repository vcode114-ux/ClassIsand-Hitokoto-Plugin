using ClassIsland.Core.Abstractions.Controls;
using HitokotoPlugin.Models;

namespace HitokotoPlugin.Views.Components;

/// <summary>
/// 一言组件实例设置控件。
/// 目前无实例级设置，控件中提示用户前往插件全局设置页配置 AI 审核功能。
/// </summary>
public partial class HitokotoComponentSettingsControl : ComponentBase<HitokotoComponentSettings>
{
    public HitokotoComponentSettingsControl()
    {
        InitializeComponent();
    }
}
