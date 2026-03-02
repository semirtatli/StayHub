using FluentValidation;

namespace StayHub.Services.Hotel.Application.Features.RemoveRoom;

/// <summary>
/// Validates RemoveRoomCommand.
/// </summary>
public sealed class RemoveRoomCommandValidator : AbstractValidator<RemoveRoomCommand>
{
    public RemoveRoomCommandValidator()
    {
        RuleFor(x => x.HotelId)
            .NotEmpty().WithMessage("Hotel ID is required.");

        RuleFor(x => x.RoomId)
            .NotEmpty().WithMessage("Room ID is required.");

        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage("Owner ID is required.");
    }
}
