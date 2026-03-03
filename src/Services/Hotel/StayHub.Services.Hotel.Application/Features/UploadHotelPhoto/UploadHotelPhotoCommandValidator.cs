using FluentValidation;

namespace StayHub.Services.Hotel.Application.Features.UploadHotelPhoto;

/// <summary>
/// Validates UploadHotelPhotoCommand.
/// </summary>
public sealed class UploadHotelPhotoCommandValidator : AbstractValidator<UploadHotelPhotoCommand>
{
    public UploadHotelPhotoCommandValidator()
    {
        RuleFor(x => x.HotelId)
            .NotEmpty().WithMessage("Hotel ID is required.");

        RuleFor(x => x.PhotoUrl)
            .NotEmpty().WithMessage("Photo URL is required.")
            .MaximumLength(2048).WithMessage("Photo URL must not exceed 2048 characters.");

        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage("Owner ID is required.");
    }
}
