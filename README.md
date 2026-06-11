# 一言 Hitokoto Plugin for ClassIsland
根据https://github.com/qbw101/Hitokoto-Plugin进行优化，可以在设置中自定义api参数（调整返回的句子参数）
在 ClassIsland 课程表软件的主界面添加一个**一言**组件，每 20 秒自动从 [hitokoto.cn](https://hitokoto.cn) 获取一句话，显示内容、来源作品及作者。

## 功能

- 📖 **一言展示**：每 20 秒自动刷新，显示一言正文、来源作品和作者
- 🤖 **豆包 AI 审核（可选）**：接入火山引擎豆包大模型，对每条一言进行内容审核，过滤不适合课堂场景的内容
- 🤖 **千问 AI 审核（可选）**：接入阿里云通义千问大模型，同样支持内容审核，与豆包互斥，二选一
- ⚙️ **插件全局设置页**：在应用设置 → 扩展设置中独立配置

## 使用方法

1. 将插件文件夹放入 ClassIsland 的 `Plugins` 目录并启动
2. 打开【应用设置】→【组件】，将"一言"拖入主界面
3. （可选）打开【应用设置】→【扩展设置】→【一言插件设置】，选择 AI 审核方案并填写对应 API Key
4. 打开【一言插件设置】，在api设置调整句子参数（c值），详细如下：
    句子类型（参数）
    参数	说明
    a	动画
    b	漫画
    c	游戏
    d	文学
    e	原创
    f	来自网络
    g	其他
    h	影视
    i	诗词
    j	网易云
    k	哲学
    l	抖机灵
    其他	作为 动画 类型处理
    可选择多个分类，例如： ?c=a&c=c

## 豆包 AI 审核配置

> ⚠️ 此功能尚未经过完整测试，可能存在 BUG，建议优先使用千问 AI 审核。

1. 访问 [火山引擎方舟控制台](https://console.volcengine.com/ark)
2. 开通豆包大模型服务，创建一个**推理接入点**（Endpoint）
3. 生成 API Key（Bearer Token）
4. 将 API Key 和接入点 ID（形如 `ep-xxxxxxxx`）填入插件设置页中的「豆包 AI 内容审核」区域

## 千问 AI 审核配置

1. 访问 [阿里云 DashScope 控制台](https://dashscope.console.aliyun.com)
2. 开通通义千问服务，创建并复制 **API Key**
3. 将 API Key 填入插件设置页中的「千问 AI（通义千问）内容审核」区域
4. 模型 ID 默认为 `qwen-turbo`，也可填写 `qwen-plus`（效果更强）等其他支持的模型

> 豆包与千问两项审核互斥，开启其中一项时另一项会自动关闭。


## 依赖

- ClassIsland >= 2.0.0
- .NET 8
- CommunityToolkit.Mvvm
"# ClassIsand-Hitokoto-Plugin"  
