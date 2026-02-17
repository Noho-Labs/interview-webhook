namespace WebhookService.Models;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Status { get; set; } = "pending";
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
