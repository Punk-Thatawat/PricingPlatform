using Pricing.Domain.PricingEngine.DomainServices.Evaluators;
using Pricing.Domain.PricingEngine.Rules;
using Pricing.Domain.PricingEngine.ValueObjects;

namespace Pricing.Domain.PricingEngine.DomainServices
{
    public sealed class CompiledPricingPipeline
    {
        private readonly PipelineStep[] _steps;

        public CompiledPricingPipeline(PipelineStep[] steps)
        {
            _steps = steps;
        }

        public decimal Execute(in PriceContext ctx)
        {
            if (_steps is null)
                throw new InvalidOperationException("Pipeline not initialized");

            decimal basePrice = ctx.BasePrice;

            decimal additive = 0;
            decimal multiplier = 1;
            decimal overridePrice = 0;
            bool hasOverride = false;

            for (int i = 0; i < _steps.Length; i++)
            {
                ref readonly var step = ref _steps[i];

                var effect = step.Rule.Type switch
                {
                    RuleType.WeightTier =>
                        WeightTierEvaluator.Execute(in ctx, in step.Rule),

                    RuleType.RemoteAreaSurcharge =>
                        RemoteAreaSurchargeEvaluator.Execute(in ctx, in step.Rule),

                    RuleType.TimeWindowPromotion =>
                        TimeWindowPromotionEvaluator.Execute(in ctx, in step.Rule),

                    _ => throw new NotSupportedException($"Unsupported rule type: {step.Rule.Type}")
                };

                additive += effect.Additive;
                multiplier *= effect.Multiplicative;

                if (effect.Override.HasValue)
                {
                    overridePrice = effect.Override.Value;
                    hasOverride = true;
                }
            }

            return hasOverride
                ? overridePrice
                : (basePrice + additive) * multiplier;
        }
    }
}
