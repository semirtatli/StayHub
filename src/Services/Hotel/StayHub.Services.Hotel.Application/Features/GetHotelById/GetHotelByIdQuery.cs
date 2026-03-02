using StayHub.Services.Hotel.Application.DTOs;
using StayHub.Shared.CQRS;

namespace StayHub.Services.Hotel.Application.Features.GetHotelById;

/// <summary>
/// Query to get a hotel by ID with all rooms.
/// Returns HotelDetailDto (includes room list) for the detail view.
///
/// Access: Public or Authenticated — anyone can view hotel details.
/// </summary>
public sealed record GetHotelByIdQuery(Guid HotelId) : IQuery<HotelDetailDto>;
