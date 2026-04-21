using FluentValidation;
using Rule.Application.DTOs;

namespace Rule.Application.Validators;

public sealed class CreateRuleRequestDtoValidator : AbstractValidator<CreateRuleRequestDto>
{
    public CreateRuleRequestDtoValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum();

        RuleFor(x => x.ConfigJson)
            .NotEmpty();

        RuleFor(x => x.Priority)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.EffectiveFrom)
            .NotEmpty();

        RuleFor(x => x)
            .Must(x => x.EffectiveTo is null || x.EffectiveTo >= x.EffectiveFrom)
            .WithMessage("EffectiveTo must be greater than or equal to EffectiveFrom.");
    }
}
