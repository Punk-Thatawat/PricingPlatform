using System.Collections.Concurrent;
using Rule.Domain.Entities;
using Rule.Domain.Repositories;

namespace Rule.Infrastructure.Repositories;

public sealed class InMemoryRuleRepository : IRuleRepository
{
    private readonly ConcurrentDictionary<Guid, RuleDefinition> _rules = new();

    public Task<IReadOnlyCollection<RuleDefinition>> GetAllAsync(bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var query = _rules.Values.AsEnumerable();

        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        IReadOnlyCollection<RuleDefinition> rules = query
            .OrderBy(x => x.Priority)
            .ThenBy(x => x.EffectiveFrom)
            .ToArray();

        return Task.FromResult(rules);
    }

    public Task<RuleDefinition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _rules.TryGetValue(id, out var rule);

        return Task.FromResult(rule);
    }

    public Task<RuleDefinition> CreateAsync(RuleDefinition rule, CancellationToken cancellationToken = default)
    {
        rule.Id = Guid.NewGuid();
        rule.Version = 1;
        rule.IsActive = false;
        _rules[rule.Id] = rule;

        return Task.FromResult(rule);
    }

    public Task<RuleDefinition?> UpdateAsync(RuleDefinition rule, CancellationToken cancellationToken = default)
    {
        if (!_rules.TryGetValue(rule.Id, out var current))
        {
            return Task.FromResult<RuleDefinition?>(null);
        }

        rule.Version = current.Version + 1;
        _rules[rule.Id] = rule;

        return Task.FromResult<RuleDefinition?>(rule);
    }

    public Task<RuleDefinition?> SetActiveStatusAsync(Guid id, bool isActive, CancellationToken cancellationToken = default)
    {
        if (!_rules.TryGetValue(id, out var current))
        {
            return Task.FromResult<RuleDefinition?>(null);
        }

        current.IsActive = isActive;
        current.Version++;

        return Task.FromResult<RuleDefinition?>(current);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var removed = _rules.TryRemove(id, out _);

        return Task.FromResult(removed);
    }
}
