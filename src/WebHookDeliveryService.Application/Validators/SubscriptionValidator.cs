using FluentValidation;
using WebHookDeliveryService.Application.DTOs;

namespace WebHookDeliveryService.Application.Validators;

public class CreateSubscriptionValidator : AbstractValidator<CreateSubscriptionRequest>
{
    public CreateSubscriptionValidator()
    {
        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("URL is required")
            .Must(x => Uri.TryCreate(x, UriKind.Absolute, out _)).WithMessage("URL must be a valid URI")
            .Must(x => x!.StartsWith("http://") || x.StartsWith("https://"))
            .WithMessage("URL must start with http:// or https://");

        RuleFor(x => x.Events)
            .NotEmpty().WithMessage("At least one event type is required");

        RuleFor(x => x.MaxRetries)
            .InclusiveBetween(1, 20).WithMessage("Max retries must be between 1 and 20");

        RuleFor(x => x.BaseDelaySeconds)
            .InclusiveBetween(1, 300).WithMessage("Base delay must be between 1 and 300 seconds");

        RuleFor(x => x.MaxDelaySeconds)
            .InclusiveBetween(10, 86400).WithMessage("Max delay must be between 10 and 86400 seconds");
    }
}

public class UpdateSubscriptionValidator : AbstractValidator<UpdateSubscriptionRequest>
{
    public UpdateSubscriptionValidator()
    {
        When(x => x.Url is not null, () =>
        {
            RuleFor(x => x.Url!)
                .Must(x => Uri.TryCreate(x, UriKind.Absolute, out _)).WithMessage("URL must be a valid URI");
        });

        When(x => x.Events is not null, () =>
        {
            RuleFor(x => x.Events!)
                .NotEmpty().WithMessage("At least one event type is required");
        });

        When(x => x.MaxRetries is not null, () =>
        {
            RuleFor(x => x.MaxRetries!.Value)
                .InclusiveBetween(1, 20).WithMessage("Max retries must be between 1 and 20");
        });
    }
}
