using System.Text.Json.Serialization;

namespace HitokotoPlugin.Models;

/// <summary>
/// https://v1.hitokoto.cn/ 的 JSON 响应结构
/// </summary>
public class HitokotoResponse
{
    [JsonPropertyName("hitokoto")]
    public string Hitokoto { get; set; } = string.Empty;

    [JsonPropertyName("from")]
    public string From { get; set; } = string.Empty;

    [JsonPropertyName("from_who")]
    public string? FromWho { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }
}
