using Rule.Domain.Entities;

namespace Rule.Domain.Repositories;

public interface IRuleRepository
{
    Task<IReadOnlyCollection<RuleDefinition>> GetAllAsync(bool? isActive = null, CancellationToken cancellationToken = default);
    Task<RuleDefinition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RuleDefinition> CreateAsync(RuleDefinition rule, CancellationToken cancellationToken = default);
    Task<RuleDefinition?> UpdateAsync(RuleDefinition rule, CancellationToken cancellationToken = default);
    Task<RuleDefinition?> SetActiveStatusAsync(Guid id, bool isActive, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
