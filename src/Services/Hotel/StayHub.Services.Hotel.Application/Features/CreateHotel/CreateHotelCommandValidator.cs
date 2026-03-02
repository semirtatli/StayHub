using FluentValidation;

namespace StayHub.Services.Hotel.Application.Features.CreateHotel;

/// <summary>
/// Validates CreateHotelCommand before it reaches the handler.
/// Runs automatically in the ValidationBehavior pipeline.
///
/// Rules:
/// - Name: required, max 200 chars
/// - Description: max 4000 chars
/// - StarRating: 1-5
/// - Address fields: required (street, city, country, zip code)
/// - Phone: required
/// - Email: required, valid format
/// - CheckInTime/CheckOutTime: valid HH:mm format if provided
/// - Latitude: -90 to 90 if provided
/// - Longitude: -180 to 180 if provided
/// - OwnerId: required (set by controller from JWT)
/// </summary>
public sealed class CreateHotelCommandValidator : AbstractValidator<CreateHotelCommand>
{
    public CreateHotelCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Hotel name is required.")
            .MaximumLength(200).WithMessage("Hotel name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(4000).WithMessage("Description must not exceed 4000 characters.");

        RuleFor(x => x.StarRating)
            .InclusiveBetween(1, 5).WithMessage("Star rating must be between 1 and 5.");

        // Address validation
        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Street is required.")
            .MaximumLength(300).WithMessage("Street must not exceed 300 characters.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(100).WithMessage("City must not exceed 100 characters.");

        RuleFor(x => x.State)
            .MaximumLength(100).WithMessage("State must not exceed 100 characters.");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required.")
            .MaximumLength(100).WithMessage("Country must not exceed 100 characters.");

        RuleFor(x => x.ZipCode)
            .NotEmpty().WithMessage("Zip code is required.")
            .MaximumLength(20).WithMessage("Zip code must not exceed 20 characters.");

        // Contact info validation
        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required.")
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Contact email is required.")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Website)
            .MaximumLength(500).WithMessage("Website URL must not exceed 500 characters.")
            .When(x => x.Website is not null);

        // Check-in / Check-out time validation
        RuleFor(x => x.CheckInTime)
            .Must(BeValidTimeOnly).WithMessage("Check-in time must be in HH:mm format (e.g., 14:00).")
            .When(x => x.CheckInTime is not null);

        RuleFor(x => x.CheckOutTime)
            .Must(BeValidTimeOnly).WithMessage("Check-out time must be in HH:mm format (e.g., 11:00).")
            .When(x => x.CheckOutTime is not null);

        // Geo location validation — if one is provided, both must be provided
        When(x => x.Latitude.HasValue || x.Longitude.HasValue, () =>
        {
            RuleFor(x => x.Latitude)
                .NotNull().WithMessage("Latitude is required when longitude is provided.")
                .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90.");

            RuleFor(x => x.Longitude)
                .NotNull().WithMessage("Longitude is required when latitude is provided.")
                .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180.");
        });

        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage("Owner ID is required.");
    }

    private static bool BeValidTimeOnly(string? time) =>
        TimeOnly.TryParseExact(time, "HH:mm", out _);
}
