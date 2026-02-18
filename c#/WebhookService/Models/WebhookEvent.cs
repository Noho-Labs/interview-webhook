namespace WebhookService.Models;

public class WebhookEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Provider { get; set; } = "stripe";
    public string EventId { get; set; } = "";
    public string Type { get; set; } = "";
    /// <summary>Full event JSON payload.</summary>
    public string Payload { get; set; } = "{}";
    public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ProcessedAt { get; set; }
    public string? ProcessingError { get; set; }
}
