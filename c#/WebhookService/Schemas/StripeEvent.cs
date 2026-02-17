using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebhookService.Schemas;

/// <summary>Mirrors python/app/schemas.py StripeEvent</summary>
public class StripeEvent
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    /// <summary>Arbitrary event data — kept simple for the exercise.</summary>
    [JsonPropertyName("data")]
    public JsonElement Data { get; set; }
}
