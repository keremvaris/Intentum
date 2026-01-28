using FluentValidation;
using Intentum.Cqrs.Web.Features.SampleFeature.Commands;

namespace Intentum.Cqrs.Web.Features.SampleFeature.Validators;

public sealed class SampleCommandValidator : AbstractValidator<SampleCommand>
{
    public SampleCommandValidator() => RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
}
