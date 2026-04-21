namespace Pricing.Application.Interfaces;

public interface IRuleServiceClient
{
    Task<IReadOnlyCollection<RuleDefinitionSnapshot>> GetActiveRulesAsync(CancellationToken cancellationToken = default);
}
