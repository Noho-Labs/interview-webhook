using System.Text.Json.Serialization;

namespace WebhookService.Schemas;

/// <summary>Mirrors python/app/schemas.py WebhookResponse</summary>
public class WebhookResponse
{
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    [JsonPropertyName("duplicate")]
    public bool Duplicate { get; set; }

    [JsonPropertyName("processed")]
    public bool Processed { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    public WebhookResponse(bool ok, bool duplicate = false, bool processed = false, string? error = null)
    {
        Ok = ok;
        Duplicate = duplicate;
        Processed = processed;
        Error = error;
    }
}
