using FluentValidation;

namespace StayHub.Services.Hotel.Application.Features.SetCancellationPolicy;

/// <summary>
/// Validates SetCancellationPolicyCommand before it reaches the handler.
/// </summary>
public sealed class SetCancellationPolicyCommandValidator : AbstractValidator<SetCancellationPolicyCommand>
{
    private static readonly string[] ValidPolicyTypes =
        ["Flexible", "Moderate", "Strict", "NonRefundable"];

    public SetCancellationPolicyCommandValidator()
    {
        RuleFor(x => x.HotelId)
            .NotEmpty().WithMessage("Hotel ID is required.");

        RuleFor(x => x.PolicyType)
            .NotEmpty().WithMessage("Policy type is required.")
            .Must(pt => ValidPolicyTypes.Contains(pt, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Policy type must be one of: Flexible, Moderate, Strict, NonRefundable.");

        When(x => x.UseCustom, () =>
        {
            RuleFor(x => x.FreeCancellationDays)
                .NotNull().WithMessage("Free cancellation days is required for custom policies.")
                .InclusiveBetween(0, 365)
                .WithMessage("Free cancellation days must be between 0 and 365.");

            RuleFor(x => x.PartialRefundPercentage)
                .NotNull().WithMessage("Partial refund percentage is required for custom policies.")
                .InclusiveBetween(0, 100)
                .WithMessage("Partial refund percentage must be between 0 and 100.");

            RuleFor(x => x.PartialRefundDays)
                .NotNull().WithMessage("Partial refund days is required for custom policies.")
                .InclusiveBetween(0, 365)
                .WithMessage("Partial refund days must be between 0 and 365.")
                .LessThanOrEqualTo(x => x.FreeCancellationDays ?? 0)
                .WithMessage("Partial refund days cannot exceed free cancellation days.");
        });

        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage("Owner ID is required.");
    }
}
