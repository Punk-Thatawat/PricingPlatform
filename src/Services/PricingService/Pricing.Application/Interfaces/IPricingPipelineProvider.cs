using Pricing.Domain.PricingEngine.DomainServices;

namespace Pricing.Application.Interfaces;

public interface IPricingPipelineProvider
{
    Task<PricingPipelineSnapshot> GetCompiledPipelineAsync(CancellationToken cancellationToken = default);
    void Invalidate();
}
