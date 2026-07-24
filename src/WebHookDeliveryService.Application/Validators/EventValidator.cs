using FluentValidation;
using WebHookDeliveryService.Application.DTOs;

namespace WebHookDeliveryService.Application.Validators;

public class EventIngestValidator : AbstractValidator<EventIngestRequest>
{
    public EventIngestValidator()
    {
        RuleFor(x => x.EventType)
            .NotEmpty().WithMessage("Event type is required")
            .MaximumLength(128).WithMessage("Event type must not exceed 128 characters");

        RuleFor(x => x.Payload)
            .NotEmpty().WithMessage("Payload is required");

        RuleFor(x => x.IdempotencyKey)
            .MaximumLength(256).WithMessage("Idempotency key must not exceed 256 characters");
    }
}
