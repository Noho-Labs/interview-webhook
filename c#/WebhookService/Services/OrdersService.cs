using Microsoft.EntityFrameworkCore;
using WebhookService.Data;

namespace WebhookService.Services;

/// <summary>Mirrors python/app/orders.py</summary>
public class OrdersService
{
    private readonly AppDbContext _db;

    public OrdersService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Marks an order as paid.
    /// Throws <see cref="KeyNotFoundException"/> with "order_not_found" if the order does not exist.
    /// </summary>
    public async Task MarkOrderPaidAsync(Guid orderId)
    {
        var order = await _db.Orders.FindAsync(orderId);
        if (order is null)
            throw new KeyNotFoundException("order_not_found");

        order.Status = "paid";
        order.UpdatedAt = DateTimeOffset.UtcNow;
    }
}
