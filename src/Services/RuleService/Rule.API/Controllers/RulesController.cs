using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Rule.Application.DTOs;
using Rule.Application.Interfaces;
using Rule.API.Extensions;
using Rule.API.Mappings;
using Rule.Domain.Entities;
using Rule.Domain.Repositories;
using Rule.Infrastructure.Observability;

namespace Rule.API.Controllers;

[ApiController]
[Route("rules")]
public sealed class RulesController : ControllerBase
{
    private readonly IRuleRepository _ruleRepository;
    private readonly IValidator<CreateRuleRequestDto> _createValidator;
    private readonly IValidator<UpdateRuleRequestDto> _updateValidator;
    private readonly IPricingServiceWebhook _pricingServiceWebhook;
    private readonly ILogger<RulesController> _logger;

    public RulesController(
        IRuleRepository ruleRepository,
        IValidator<CreateRuleRequestDto> createValidator,
        IValidator<UpdateRuleRequestDto> updateValidator,
        IPricingServiceWebhook pricingServiceWebhook,
        ILogger<RulesController> logger)
    {
        _ruleRepository = ruleRepository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _pricingServiceWebhook = pricingServiceWebhook;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<RuleDto>>> GetAll([FromQuery] bool? isActive, CancellationToken cancellationToken)
    {
        using var activity = RuleInstrumentation.StartActivity("rule.rules.list");
        activity?.SetTag("rules.is_active_filter", isActive);
        var rules = await _ruleRepository.GetAllAsync(isActive, cancellationToken);
        activity?.SetTag("rules.count", rules.Count);

        return Ok(rules.Select(x => x.MapToDto()).ToArray());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RuleDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var rule = await _ruleRepository.GetByIdAsync(id, cancellationToken);
        if (rule is null)
        {
            return NotFound();
        }

        return Ok(rule.MapToDto());
    }

    [HttpPost]
    public async Task<ActionResult<RuleDto>> Create(CreateRuleRequestDto request, CancellationToken cancellationToken)
    {
        using var activity = RuleInstrumentation.StartActivity("rule.rules.create");
        var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            validationResult.AddToModelState(ModelState);
            return ValidationProblem(ModelState);
        }

        var rule = new RuleDefinition
        {
            Type = request.Type,
            IsActive = request.IsActive,
            ConfigJson = request.ConfigJson,
            Priority = request.Priority,
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo
        };

        var created = await _ruleRepository.CreateAsync(rule, cancellationToken);
        RuleInstrumentation.RulesCreated.Add(1);
        activity?.SetTag("rule.id", created.Id);
        activity?.SetTag("rule.type", created.Type.ToString());
        await NotifyWebhookAsync(created.Id, "created", cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created.MapToDto());
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<RuleDto>> Update(Guid id, UpdateRuleRequestDto request, CancellationToken cancellationToken)
    {
        using var activity = RuleInstrumentation.StartActivity("rule.rules.update");
        activity?.SetTag("rule.id", id);
        var validationResult = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            validationResult.AddToModelState(ModelState);
            return ValidationProblem(ModelState);
        }

        var existing = await _ruleRepository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return NotFound();
        }

        existing.Type = request.Type;
        existing.IsActive = request.IsActive;
        existing.ConfigJson = request.ConfigJson;
        existing.Priority = request.Priority;
        existing.EffectiveFrom = request.EffectiveFrom;
        existing.EffectiveTo = request.EffectiveTo;

        var updated = await _ruleRepository.UpdateAsync(existing, cancellationToken);
        RuleInstrumentation.RulesUpdated.Add(1);
        activity?.SetTag("rule.type", updated!.Type.ToString());
        await NotifyWebhookAsync(updated!.Id, "updated", cancellationToken);

        return Ok(updated!.MapToDto());
    }

    [HttpPatch("{id:guid}/activate")]
    public async Task<ActionResult<RuleDto>> Activate(Guid id, CancellationToken cancellationToken)
    {
        using var activity = RuleInstrumentation.StartActivity("rule.rules.activate");
        activity?.SetTag("rule.id", id);
        var updated = await _ruleRepository.SetActiveStatusAsync(id, true, cancellationToken);
        if (updated is null)
        {
            return NotFound();
        }

        RuleInstrumentation.RulesActivated.Add(1);
        await NotifyWebhookAsync(updated.Id, "activated", cancellationToken);

        return Ok(updated.MapToDto());
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<ActionResult<RuleDto>> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        using var activity = RuleInstrumentation.StartActivity("rule.rules.deactivate");
        activity?.SetTag("rule.id", id);
        var updated = await _ruleRepository.SetActiveStatusAsync(id, false, cancellationToken);
        if (updated is null)
        {
            return NotFound();
        }

        RuleInstrumentation.RulesDeactivated.Add(1);
        await NotifyWebhookAsync(updated.Id, "deactivated", cancellationToken);

        return Ok(updated.MapToDto());
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        using var activity = RuleInstrumentation.StartActivity("rule.rules.delete");
        activity?.SetTag("rule.id", id);
        var removed = await _ruleRepository.DeleteAsync(id, cancellationToken);
        if (!removed)
        {
            return NotFound();
        }

        RuleInstrumentation.RulesDeleted.Add(1);
        return NoContent();
    }

    private async Task NotifyWebhookAsync(Guid ruleId, string action, CancellationToken cancellationToken)
    {
        var notified = await _pricingServiceWebhook.NotifyRuleChangedAsync(ruleId, action, cancellationToken);
        if (!notified)
        {
            _logger.LogWarning("Pricing webhook notification failed for rule {RuleId} with action {Action}.", ruleId, action);
        }
    }
}
