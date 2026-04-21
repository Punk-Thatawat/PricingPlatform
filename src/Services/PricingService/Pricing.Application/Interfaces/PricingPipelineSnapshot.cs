using Pricing.Domain.PricingEngine.DomainServices;

namespace Pricing.Application.Interfaces;

public sealed class PricingPipelineSnapshot
{
    public required CompiledPricingPipeline Pipeline { get; init; }
    public required IReadOnlyCollection<RuleDefinitionSnapshot> Rules { get; init; }
    public required DateTime CompiledAtUtc { get; init; }
}
