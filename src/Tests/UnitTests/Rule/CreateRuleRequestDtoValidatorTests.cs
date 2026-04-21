using Rule.Application.DTOs;
using Rule.Application.Validators;
using Rule.Domain.Enums;

namespace PricingPlatform.UnitTests.Rule;

public sealed class CreateRuleRequestDtoValidatorTests
{
    [Fact]
    public async Task ValidateAsync_WithEffectiveToBeforeEffectiveFrom_ReturnsValidationError()
    {
        var validator = new CreateRuleRequestDtoValidator();
        var request = new CreateRuleRequestDto
        {
            Type = RuleType.WeightTier,
            ConfigJson = "{}",
            Priority = 1,
            EffectiveFrom = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc),
            EffectiveTo = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        var result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorMessage.Contains("EffectiveTo"));
    }

    [Fact]
    public async Task ValidateAsync_WithValidPayload_Succeeds()
    {
        var validator = new UpdateRuleRequestDtoValidator();
        var request = new UpdateRuleRequestDto
        {
            Type = RuleType.RemoteAreaSurcharge,
            IsActive = true,
            ConfigJson = "{\"zones\":[\"A\"],\"surchargeType\":0,\"value\":50}",
            Priority = 0,
            EffectiveFrom = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            EffectiveTo = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc)
        };

        var result = await validator.ValidateAsync(request);

        Assert.True(result.IsValid);
    }
}
