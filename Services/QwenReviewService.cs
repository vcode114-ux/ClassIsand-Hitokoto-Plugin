using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HitokotoPlugin.Services;

/// <summary>
/// 千问 AI（通义千问）内容审核服务。
/// 使用阿里云 DashScope OpenAI 兼容接口：
///   https://dashscope.console.aliyun.com/apiKey
///   https://dashscope.aliyuncs.com/api/v1/services/aigc/text-generation/generation
/// </summary>
public class QwenReviewService
{
    private const string ApiEndpoint = "https://dashscope.aliyuncs.com/compatible-mode/v1/chat/completions";

    private static readonly HttpClient _httpClient = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(15)
    };

    /// <summary>
    /// 对一言内容进行审核。
    /// </summary>
    /// <param name="apiKey">阿里云 DashScope API Key</param>
    /// <param name="modelId">DashScope 模型 ID（如 qwen-turbo、qwen-plus 等）</param>
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
            return (false, "未配置千问 API Key 或模型 ID，请在插件设置中填写。");

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
                return (false, $"千问 API 请求失败：{response.StatusCode}");

            using var doc = JsonDocument.Parse(responseJson);

            // 尝试多种可能的内容路径
            string content = string.Empty;
            try
            {
                content = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? string.Empty;
            }
            catch
            {
                // 某些千问模型使用 output.text
                try
                {
                    content = doc.RootElement
                        .GetProperty("output")
                        .GetProperty("text")
                        .GetString() ?? string.Empty;
                }
                catch
                {
                    return (false, $"千问 API 响应格式异常：{responseJson}");
                }
            }

            var trimmed = content.Trim();
            if (trimmed.StartsWith("通过", StringComparison.OrdinalIgnoreCase))
                return (true, "千问 AI 审核通过");

            return (false, trimmed.Length > 0 ? trimmed : "千问 AI 审核拒绝");
        }
        catch (Exception ex)
        {
            return (false, $"千问 AI 审核异常：{ex.Message}");
        }
    }
}
