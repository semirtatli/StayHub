using StayHub.Services.Hotel.Application.DTOs;
using StayHub.Shared.CQRS;

namespace StayHub.Services.Hotel.Application.Features.GetRoomsByHotel;

/// <summary>
/// Query to get all rooms for a specific hotel.
/// Public endpoint — anyone can view rooms for a hotel.
/// </summary>
public sealed record GetRoomsByHotelQuery(Guid HotelId) : IQuery<IReadOnlyList<RoomDto>>;
