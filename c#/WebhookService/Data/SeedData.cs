using WebhookService.Models;

namespace WebhookService.Data;

/// <summary>
/// Populates the database with test orders (mirrors python/seed.py).
/// Run via: dotnet run -- seed
/// </summary>
public static class SeedData
{
    public static async Task RunAsync(AppDbContext db)
    {
        Console.WriteLine("🌱 Seeding database with test orders...\n");

        var testOrders = new[]
        {
            new Order { Id = new Guid("11111111-1111-1111-1111-111111111111"), Status = "pending" },
            new Order { Id = new Guid("22222222-2222-2222-2222-222222222222"), Status = "paid" },
            new Order { Id = new Guid("33333333-3333-3333-3333-333333333333"), Status = "pending" },
        };

        var existingCount = db.Orders.Count();
        if (existingCount > 0)
        {
            Console.WriteLine($"⚠️  Database already has {existingCount} order(s).");
            Console.Write("Do you want to clear and reseed? (y/N): ");
            var response = Console.ReadLine();
            if (response?.ToLower() != "y")
            {
                Console.WriteLine("❌ Seed cancelled.");
                return;
            }

            db.Orders.RemoveRange(db.Orders);
            await db.SaveChangesAsync();
            Console.WriteLine("✅ Cleared existing orders.");
        }

        db.Orders.AddRange(testOrders);
        await db.SaveChangesAsync();

        Console.WriteLine("\n✅ Successfully seeded orders:");
        Console.WriteLine("\n┌─────────────────────────────────────────┬──────────┐");
        Console.WriteLine("│ Order ID                                │ Status   │");
        Console.WriteLine("├─────────────────────────────────────────┼──────────┤");
        foreach (var order in testOrders)
            Console.WriteLine($"│ {order.Id} │ {order.Status,-8} │");
        Console.WriteLine("└─────────────────────────────────────────┴──────────┘");

        Console.WriteLine("""

📝 Example webhook payload:
curl -X POST http://localhost:9000/webhooks/stripe \
  -H "Content-Type: application/json" \
  -d '{
    "id": "evt_test_001",
    "type": "payment_intent.succeeded",
    "data": {
      "object": {
        "metadata": {
          "order_id": "11111111-1111-1111-1111-111111111111"
        }
      }
    }
  }'
""");
    }
}
