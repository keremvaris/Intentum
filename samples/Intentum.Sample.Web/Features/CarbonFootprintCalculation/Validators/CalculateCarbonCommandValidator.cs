using FluentValidation;
using Intentum.Sample.Web.Features.CarbonFootprintCalculation.Commands;

namespace Intentum.Sample.Web.Features.CarbonFootprintCalculation.Validators;

public sealed class CalculateCarbonCommandValidator : AbstractValidator<CalculateCarbonCommand>
{
    public CalculateCarbonCommandValidator()
    {
        RuleFor(x => x.Actor).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Scope).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EstimatedTonsCo2).InclusiveBetween(0, 1_000_000).When(x => x.EstimatedTonsCo2.HasValue);
    }
}
