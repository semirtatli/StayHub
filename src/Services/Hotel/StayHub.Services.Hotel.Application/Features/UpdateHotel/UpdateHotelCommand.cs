using StayHub.Services.Hotel.Application.DTOs;
using StayHub.Shared.CQRS;

namespace StayHub.Services.Hotel.Application.Features.UpdateHotel;

/// <summary>
/// Command to update an existing hotel's basic information.
/// Only allowed for hotels in Draft or Active status.
///
/// The handler verifies that the authenticated user (OwnerId) owns the hotel.
/// OwnerId is set by the controller from JWT claims.
/// </summary>
public sealed record UpdateHotelCommand(
    Guid HotelId,

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

    // Times
    string CheckInTime,
    string CheckOutTime,

    // Optional geo location
    double? Latitude,
    double? Longitude,

    // Set by controller from JWT
    string OwnerId) : ICommand<HotelDto>;
