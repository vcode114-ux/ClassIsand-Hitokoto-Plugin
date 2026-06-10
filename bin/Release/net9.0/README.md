# 一言 Hitokoto Plugin for ClassIsland

在 ClassIsland 课程表软件的主界面添加一个**一言**组件，每 20 秒自动从 [hitokoto.cn](https://hitokoto.cn) 获取一句话，显示内容、来源作品及作者。

## 功能

- 📖 **一言展示**：每 20 秒自动刷新，显示一言正文、来源作品和作者
- 🤖 **豆包 AI 审核（可选）**：接入火山引擎豆包大模型，对每条一言进行内容审核，过滤不适合课堂场景的内容
- ⚙️ **插件全局设置页**：在应用设置 → 扩展设置中独立配置

## 使用方法

1. 将插件文件夹放入 ClassIsland 的 `Plugins` 目录并启动
2. 打开【应用设置】→【组件】，将"一言"拖入主界面
3. （可选）打开【应用设置】→【扩展设置】→【一言插件设置】，填写豆包 API Key 并开启 AI 审核

## 豆包 AI 审核配置

1. 访问 [火山引擎方舟控制台](https://console.volcengine.com/ark)
2. 开通豆包大模型服务，创建一个**推理接入点**（Endpoint）
3. 生成 API Key（Bearer Token）
4. 将 API Key 和接入点 ID（形如 `ep-xxxxxxxx`）填入插件设置页

## 构建

```bash
dotnet build
```

## 依赖

- ClassIsland >= 2.0.0
- .NET 9
- CommunityToolkit.Mvvm
