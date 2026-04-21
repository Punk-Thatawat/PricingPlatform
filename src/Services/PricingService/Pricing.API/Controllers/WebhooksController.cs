using Microsoft.AspNetCore.Mvc;
using Pricing.Application.Interfaces;

namespace Pricing.API.Controllers;

[ApiController]
[Route("webhooks")]
public sealed class WebhooksController : ControllerBase
{
    private readonly ILogger<WebhooksController> _logger;
    private readonly IPricingPipelineProvider _pricingPipelineProvider;

    public WebhooksController(
        ILogger<WebhooksController> logger,
        IPricingPipelineProvider pricingPipelineProvider)
    {
        _logger = logger;
        _pricingPipelineProvider = pricingPipelineProvider;
    }

    [HttpPost("rules/changed")]
    public IActionResult RuleChanged([FromBody] RuleChangedWebhookRequest request)
    {
        _pricingPipelineProvider.Invalidate();

        _logger.LogInformation(
            "Received rule changed webhook. RuleId: {RuleId}, Action: {Action}, OccurredAtUtc: {OccurredAtUtc}",
            request.RuleId,
            request.Action,
            request.OccurredAtUtc);

        return Ok(new
        {
            accepted = true
        });
    }
}

public sealed class RuleChangedWebhookRequest
{
    public Guid RuleId { get; set; }
    public string Action { get; set; } = string.Empty;
    public DateTime OccurredAtUtc { get; set; }
}
