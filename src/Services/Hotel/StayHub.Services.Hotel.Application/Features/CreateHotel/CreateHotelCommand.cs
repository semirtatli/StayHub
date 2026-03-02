using StayHub.Services.Hotel.Application.DTOs;
using StayHub.Shared.CQRS;

namespace StayHub.Services.Hotel.Application.Features.CreateHotel;

/// <summary>
/// Command to create a new hotel listing.
/// Returns the created hotel DTO on success.
///
/// Flow: Controller → MediatR pipeline (Validation → Logging → Transaction → Handler)
///       → creates HotelEntity aggregate → persisted via TransactionBehavior.
///
/// OwnerId is set by the controller from the authenticated user's JWT claims,
/// not from the request body — prevents users from creating hotels for other accounts.
/// </summary>
public sealed record CreateHotelCommand(
    string Name,
    string Description,
    int StarRating,

    // Address fields
    string Street,
    string City,
    string State,
    string Country,
    string ZipCode,

    // Contact info
    string Phone,
    string Email,
    string? Website,

    // Optional fields
    string? CheckInTime,
    string? CheckOutTime,
    double? Latitude,
    double? Longitude,

    // Set by controller, not request body
    string OwnerId) : ICommand<HotelDto>;
