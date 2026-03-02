using StayHub.Shared.Result;

namespace StayHub.Services.Hotel.Application;

/// <summary>
/// Static error definitions for the Hotel bounded context.
/// Follows the pattern: "{Entity}.{ErrorType}" for consistent error codes.
/// </summary>
public static class HotelErrors
{
    public static class Hotel
    {
        public static readonly Error NotFound = new(
            "Hotel.NotFound",
            "Hotel was not found.");

        public static readonly Error DuplicateName = new(
            "Hotel.DuplicateName",
            "A hotel with this name already exists for this owner.");

        public static readonly Error InvalidStatus = new(
            "Hotel.InvalidStatus",
            "The operation is not allowed for the hotel's current status.");

        public static readonly Error NotOwner = new(
            "Hotel.NotOwner",
            "You are not the owner of this hotel.");

        public static readonly Error UpdateFailed = new(
            "Hotel.UpdateFailed",
            "Failed to update hotel information.");
    }

    public static class Room
    {
        public static readonly Error NotFound = new(
            "Room.NotFound",
            "Room was not found.");

        public static readonly Error DuplicateName = new(
            "Room.DuplicateName",
            "A room with this name already exists in this hotel.");

        public static readonly Error InvalidPrice = new(
            "Room.InvalidPrice",
            "Room price must be a positive amount.");

        public static readonly Error InvalidOccupancy = new(
            "Room.InvalidOccupancy",
            "Room occupancy must be at least 1.");
    }
}
