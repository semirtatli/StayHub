using FluentValidation;
using StayHub.Services.Hotel.Domain.Enums;

namespace StayHub.Services.Hotel.Application.Features.UpdateRoom;

/// <summary>
/// Validates UpdateRoomCommand before it reaches the handler.
/// Same rules as AddRoom, plus RoomId required.
/// </summary>
public sealed class UpdateRoomCommandValidator : AbstractValidator<UpdateRoomCommand>
{
    public UpdateRoomCommandValidator()
    {
        RuleFor(x => x.HotelId)
            .NotEmpty().WithMessage("Hotel ID is required.");

        RuleFor(x => x.RoomId)
            .NotEmpty().WithMessage("Room ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Room name is required.")
            .MaximumLength(100).WithMessage("Room name must not exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");

        RuleFor(x => x.RoomType)
            .NotEmpty().WithMessage("Room type is required.")
            .Must(BeValidRoomType).WithMessage("Room type must be a valid value (Single, Double, Twin, Suite, Deluxe, Family, Dormitory, Studio, Penthouse).");

        RuleFor(x => x.MaxOccupancy)
            .GreaterThanOrEqualTo(1).WithMessage("Max occupancy must be at least 1.")
            .LessThanOrEqualTo(20).WithMessage("Max occupancy must not exceed 20.");

        RuleFor(x => x.BasePrice)
            .GreaterThan(0).WithMessage("Base price must be a positive amount.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(3).WithMessage("Currency must be a 3-letter ISO 4217 code (e.g., USD, EUR).");

        RuleFor(x => x.TotalInventory)
            .GreaterThanOrEqualTo(1).WithMessage("Total inventory must be at least 1.");

        RuleFor(x => x.SizeInSquareMeters)
            .GreaterThan(0).WithMessage("Room size must be a positive number.")
            .When(x => x.SizeInSquareMeters.HasValue);

        RuleForEach(x => x.Amenities)
            .NotEmpty().WithMessage("Amenity name cannot be empty.")
            .MaximumLength(100).WithMessage("Amenity name must not exceed 100 characters.")
            .When(x => x.Amenities is not null);

        RuleForEach(x => x.PhotoUrls)
            .NotEmpty().WithMessage("Photo URL cannot be empty.")
            .MaximumLength(2000).WithMessage("Photo URL must not exceed 2000 characters.")
            .When(x => x.PhotoUrls is not null);

        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage("Owner ID is required.");
    }

    private static bool BeValidRoomType(string roomType) =>
        Enum.TryParse<RoomType>(roomType, ignoreCase: true, out _);
}
