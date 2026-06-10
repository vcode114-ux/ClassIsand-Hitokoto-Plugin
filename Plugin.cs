using System.IO;
using ClassIsland.Core.Abstractions;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Extensions.Registry;
using ClassIsland.Shared.Helpers;
using HitokotoPlugin.Models;
using HitokotoPlugin.Views.Components;
using HitokotoPlugin.Views.SettingsPages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HitokotoPlugin;

[PluginEntrance]
public class Plugin : PluginBase
{
    public PluginSettings Settings { get; private set; } = new();

    private string SettingsFilePath => Path.Combine(PluginConfigFolder, "Settings.json");

    public override void Initialize(HostBuilderContext context, IServiceCollection services)
    {
        // 1. 加载持久化配置
        Settings = ConfigureFileHelper.LoadConfig<PluginSettings>(SettingsFilePath);
        Settings.PropertyChanged += (_, _) =>
            ConfigureFileHelper.SaveConfig<PluginSettings>(SettingsFilePath, Settings);

        // 2. 注册为单例，设置页和组件均可通过构造注入或服务定位器获取
        services.AddSingleton(Settings);

        // 3. 注册一言组件（含实例设置控件）
        services.AddComponent<HitokotoComponent, HitokotoComponentSettingsControl>();

        // 4. 注册插件全局设置页
        services.AddSettingsPage<HitokotoSettingsPage>();
    }

    public void OnShutdown()
    {
        ConfigureFileHelper.SaveConfig<PluginSettings>(SettingsFilePath, Settings);
    }
}
