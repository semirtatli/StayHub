using System.Globalization;
using StayHub.Services.Hotel.Domain.Entities;

namespace StayHub.Services.Hotel.Application.DTOs;

/// <summary>
/// Centralized mapping from domain entities to DTOs.
/// Uses manual mapping for explicitness — no reflection-based mapper overhead.
/// </summary>
public static class HotelMappings
{
    public static HotelDto ToDto(HotelEntity hotel) => new(
        hotel.Id,
        hotel.Name,
        hotel.Description,
        hotel.StarRating,
        ToAddressDto(hotel.Address),
        hotel.Location is not null ? new GeoLocationDto(hotel.Location.Latitude, hotel.Location.Longitude) : null,
        new ContactInfoDto(hotel.ContactInfo.Phone, hotel.ContactInfo.Email, hotel.ContactInfo.Website),
        hotel.OwnerId,
        hotel.Status.ToString(),
        hotel.StatusReason,
        hotel.CheckInTime.ToString("HH:mm", CultureInfo.InvariantCulture),
        hotel.CheckOutTime.ToString("HH:mm", CultureInfo.InvariantCulture),
        hotel.CoverImageUrl,
        hotel.PhotoUrls.ToList(),
        hotel.Rooms.Count,
        hotel.CreatedAt,
        hotel.LastModifiedAt);

    public static HotelDetailDto ToDetailDto(HotelEntity hotel) => new(
        hotel.Id,
        hotel.Name,
        hotel.Description,
        hotel.StarRating,
        ToAddressDto(hotel.Address),
        hotel.Location is not null ? new GeoLocationDto(hotel.Location.Latitude, hotel.Location.Longitude) : null,
        new ContactInfoDto(hotel.ContactInfo.Phone, hotel.ContactInfo.Email, hotel.ContactInfo.Website),
        hotel.OwnerId,
        hotel.Status.ToString(),
        hotel.StatusReason,
        hotel.CheckInTime.ToString("HH:mm", CultureInfo.InvariantCulture),
        hotel.CheckOutTime.ToString("HH:mm", CultureInfo.InvariantCulture),
        hotel.CoverImageUrl,
        hotel.PhotoUrls.ToList(),
        hotel.Rooms.Select(ToRoomDto).ToList(),
        hotel.CreatedAt,
        hotel.LastModifiedAt);

    public static HotelSummaryDto ToSummaryDto(HotelEntity hotel) => new(
        hotel.Id,
        hotel.Name,
        hotel.StarRating,
        hotel.Address.City,
        hotel.Address.Country,
        hotel.Status.ToString(),
        hotel.CoverImageUrl,
        hotel.Rooms.Count,
        hotel.Rooms.Count > 0 ? hotel.Rooms.Min(r => r.BasePrice.Amount) : null,
        hotel.Rooms.Count > 0 ? hotel.Rooms[0].BasePrice.Currency : null,
        hotel.PhotoUrls.Count,
        hotel.CreatedAt);

    public static RoomDto ToRoomDto(Room room) => new(
        room.Id,
        room.HotelId,
        room.Name,
        room.Description,
        room.RoomType.ToString(),
        room.MaxOccupancy,
        room.BasePrice.Amount,
        room.BasePrice.Currency,
        room.TotalInventory,
        room.SizeInSquareMeters,
        room.BedConfiguration,
        room.IsActive,
        room.Amenities,
        room.PhotoUrls);

    private static AddressDto ToAddressDto(Domain.ValueObjects.Address address) => new(
        address.Street,
        address.City,
        address.State,
        address.Country,
        address.ZipCode);
}
