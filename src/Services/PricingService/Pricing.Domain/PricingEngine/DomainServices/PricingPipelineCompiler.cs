using Pricing.Domain.PricingEngine.Rules;
using Pricing.Domain.PricingEngine.ValueObjects;

namespace Pricing.Domain.PricingEngine.DomainServices
{
    public sealed class PricingPipelineCompiler
    {
        public CompiledPricingPipeline Compile(IReadOnlyList<Rule> rules)
        {
            var steps = new PipelineStep[rules.Count];

            for (int i = 0; i < rules.Count; i++)
            {
                var rule = rules[i];
                steps[i] = new PipelineStep(in rule);
            }

            return new CompiledPricingPipeline(steps);
        }
    }
}
