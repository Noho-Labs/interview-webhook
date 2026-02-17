/*
 * Tests for the Stripe webhook endpoint.
 *
 * TODO (Candidate):
 * Part A - Implement at least ONE test for the happy path
 * Part B - Add a test for duplicate webhook handling (most important!)
 *
 * You don't need to implement all the test stubs below - focus on what matters.
 */

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebhookService.Data;
using WebhookService.Models;
using Xunit;

namespace WebhookService.Tests;

/// <summary>
/// Integration tests using an in-memory SQLite database.
/// Mirrors python/app/tests/test_webhook.py
/// </summary>
public class WebhookTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public WebhookTests()
    {
        // Keep a shared in-memory SQLite connection alive for the test lifetime
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the real DbContext registration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (descriptor is not null)
                        services.Remove(descriptor);

                    // Register a test DbContext backed by the shared in-memory connection
                    services.AddDbContext<AppDbContext>(options =>
                        options.UseSqlite(_connection));
                });
            });

        _client = _factory.CreateClient();

        // Create fresh tables (mirrors setup_database autouse fixture)
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        // Drop all tables after the test (mirrors Base.metadata.drop_all)
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureDeleted();

        _factory.Dispose();
        _connection.Dispose();
    }

    // =========================================================================
    // Helpers — mirrors the fixture helpers at the bottom of test_webhook.py
    // =========================================================================

    private Order CreateTestOrder(Guid? id = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var order = new Order
        {
            Id = id ?? new Guid("11111111-1111-1111-1111-111111111111"),
            Status = "pending",
        };
        db.Orders.Add(order);
        db.SaveChanges();
        return order;
    }

    private void AssertOrderPaid(Guid orderId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var order = db.Orders.Find(orderId);
        Assert.NotNull(order);
        Assert.Equal("paid", order.Status);
    }

    private WebhookEvent? AssertWebhookStored(string eventId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var webhook = db.WebhookEvents.FirstOrDefault(e => e.EventId == eventId);
        Assert.NotNull(webhook);
        return webhook;
    }

    private int CountWebhooksForEvent(string eventId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return db.WebhookEvents.Count(e => e.EventId == eventId);
    }

    // =========================================================================
    // Part A Tests — Base webhook functionality
    // =========================================================================

    [Fact(Skip = "TODO: Implement this test")]
    public async Task WebhookHappyPath_ReturnsOkAndUpdatesOrder()
    {
        /*
         * PRIORITY: Implement this test first!
         *
         * Verify that:
         * - POST to /webhooks/stripe returns 200 OK
         * - Order status changes to 'paid'
         * - Webhook event is stored in database
         */
        var testOrder = CreateTestOrder();

        var payload = new
        {
            id = "evt_test_001",
            type = "payment_intent.succeeded",
            data = new
            {
                @object = new
                {
                    metadata = new
                    {
                        order_id = testOrder.Id.ToString()
                    }
                }
            }
        };

        // TODO: Implement this test
        // var response = await _client.PostAsJsonAsync("/webhooks/stripe", payload);
        // Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        // AssertOrderPaid(testOrder.Id);
        // AssertWebhookStored("evt_test_001");
    }

    // Optional: Add more edge case tests if you have time
    // [Fact]
    // public async Task WebhookMissingOrderId_HandlesGracefully() { }
    //
    // [Fact]
    // public async Task WebhookOrderNotFound_HandlesGracefully() { }

    // =========================================================================
    // Part B Tests — Idempotency
    // =========================================================================

    [Fact(Skip = "TODO: Implement in Part B")]
    public async Task DuplicateWebhook_IsIdempotent()
    {
        /*
         * PART B - CRITICAL TEST: Implement this after Part A works!
         *
         * Verify idempotency by sending the same webhook twice:
         * - Both requests return 200 OK
         * - Only ONE webhook_event record exists
         * - Order is in correct state (not corrupted)
         */
        var testOrder = CreateTestOrder();

        var payload = new
        {
            id = "evt_duplicate_001",
            type = "payment_intent.succeeded",
            data = new
            {
                @object = new
                {
                    metadata = new
                    {
                        order_id = testOrder.Id.ToString()
                    }
                }
            }
        };

        // TODO: Implement this test for Part B
        // var response1 = await _client.PostAsJsonAsync("/webhooks/stripe", payload);
        // var response2 = await _client.PostAsJsonAsync("/webhooks/stripe", payload);
        // Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        // Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        // Assert.Equal(1, CountWebhooksForEvent("evt_duplicate_001"));
        // AssertOrderPaid(testOrder.Id);
    }

    // Optional: Only if you finish Part B with time to spare
    // [Fact]
    // public async Task ConcurrentDuplicateWebhooks_OnlyProcessOnce() { }
}
