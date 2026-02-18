using Microsoft.AspNetCore.Mvc;
using WebhookService.Data;
using WebhookService.Schemas;
using WebhookService.Services;

namespace WebhookService.Controllers;

[ApiController]
[Route("webhooks")]
public class WebhooksController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly OrdersService _ordersService;

    public WebhooksController(AppDbContext db, OrdersService ordersService)
    {
        _db = db;
        _ordersService = ordersService;
    }

    /// <summary>
    /// Implement webhook handling for Stripe payment events.
    ///
    /// Part A: Basic webhook processing
    /// Part B: Handle duplicate events (introduced at ~20 min)
    ///
    /// See README.md for detailed requirements.
    /// </summary>
    [HttpPost("stripe")]
    public async Task<ActionResult<WebhookResponse>> StripeWebhook([FromBody] StripeEvent stripeEvent)
    {
        // TODO: Implement webhook handling here

        return Ok(new WebhookResponse(ok: true));
    }
}
