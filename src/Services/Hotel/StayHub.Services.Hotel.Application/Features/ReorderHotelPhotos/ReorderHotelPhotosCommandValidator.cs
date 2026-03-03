using FluentValidation;

namespace StayHub.Services.Hotel.Application.Features.ReorderHotelPhotos;

/// <summary>
/// Validates ReorderHotelPhotosCommand.
/// </summary>
public sealed class ReorderHotelPhotosCommandValidator : AbstractValidator<ReorderHotelPhotosCommand>
{
    public ReorderHotelPhotosCommandValidator()
    {
        RuleFor(x => x.HotelId)
            .NotEmpty().WithMessage("Hotel ID is required.");

        RuleFor(x => x.PhotoUrls)
            .NotNull().WithMessage("Photo URLs list is required.")
            .Must(urls => urls.Count > 0).WithMessage("Photo URLs list cannot be empty.");

        RuleForEach(x => x.PhotoUrls)
            .NotEmpty().WithMessage("Photo URL cannot be empty.");

        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage("Owner ID is required.");
    }
}
