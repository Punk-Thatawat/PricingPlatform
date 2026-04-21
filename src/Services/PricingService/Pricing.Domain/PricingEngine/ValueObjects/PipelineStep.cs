using Pricing.Domain.PricingEngine.Rules;

namespace Pricing.Domain.PricingEngine.ValueObjects
{
    public readonly struct PipelineStep
    {
        public readonly Rule Rule;

        public PipelineStep(in Rule rule)
        {
            Rule = rule;
        }
    }
}
