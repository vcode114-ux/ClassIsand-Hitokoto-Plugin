using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HitokotoPlugin.Services;

/// <summary>
/// 豆包 AI 内容审核服务。
/// 豆包平台兼容 OpenAI Chat Completions API，端点为：
///   https://ark.cn-beijing.volces.com/api/v3/chat/completions
/// 需要在火山引擎控制台创建推理接入点，将接入点 ID 作为 model 参数传入。
/// </summary>
public class DoubaoReviewService
{
    private const string ApiEndpoint = "https://ark.cn-beijing.volces.com/api/v3/chat/completions";

    private static readonly HttpClient _httpClient = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(15)
    };

    /// <summary>
    /// 对一言内容进行审核。
    /// </summary>
    /// <param name="apiKey">用户的豆包 API Key</param>
    /// <param name="modelId">火山引擎推理接入点 ID（如 ep-xxxxxxxxxx）</param>
    /// <param name="hitokoto">一言正文</param>
    /// <param name="from">来源作品</param>
    /// <param name="fromWho">作者</param>
    /// <returns>审核通过返回 true；被拦截或请求失败返回 false，并附带原因字符串。</returns>
    public static async Task<(bool Passed, string Reason)> ReviewAsync(
        string apiKey,
        string modelId,
        string hitokoto,
        string from,
        string? fromWho)
    {
        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(modelId))
            return (false, "未配置豆包 API Key 或模型 ID，请在插件设置中填写。");

        var prompt =
            $"请判断以下一言内容是否适合在学校课堂场景的课程表软件中向学生展示。" +
            $"要求：内容应当积极、正面，不包含暴力、色情、政治敏感等不当内容。\n\n" +
            $"一言内容：{hitokoto}\n" +
            $"来源：{from}" +
            (string.IsNullOrWhiteSpace(fromWho) ? "" : $"（{fromWho}）") +
            $"\n\n请直接回答 \"通过\" 或 \"拒绝\"，如果拒绝请简要说明原因（不超过30字）。";

        var requestBody = new
        {
            model = modelId,
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            max_tokens = 64,
            temperature = 0.0
        };

        var json = JsonSerializer.Serialize(requestBody);
        using var request = new HttpRequestMessage(HttpMethod.Post, ApiEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.SendAsync(request);
            var responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return (false, $"AI API 请求失败：{response.StatusCode}");

            using var doc = JsonDocument.Parse(responseJson);
            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;

            var trimmed = content.Trim();
            if (trimmed.StartsWith("通过", StringComparison.OrdinalIgnoreCase))
                return (true, "AI 审核通过");

            return (false, trimmed.Length > 0 ? trimmed : "AI 审核拒绝");
        }
        catch (Exception ex)
        {
            return (false, $"AI 审核异常：{ex.Message}");
        }
    }
}
