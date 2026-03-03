using FluentValidation;

namespace StayHub.Services.Hotel.Application.Features.DeleteHotelPhoto;

/// <summary>
/// Validates DeleteHotelPhotoCommand.
/// </summary>
public sealed class DeleteHotelPhotoCommandValidator : AbstractValidator<DeleteHotelPhotoCommand>
{
    public DeleteHotelPhotoCommandValidator()
    {
        RuleFor(x => x.HotelId)
            .NotEmpty().WithMessage("Hotel ID is required.");

        RuleFor(x => x.PhotoUrl)
            .NotEmpty().WithMessage("Photo URL is required.");

        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage("Owner ID is required.");
    }
}
