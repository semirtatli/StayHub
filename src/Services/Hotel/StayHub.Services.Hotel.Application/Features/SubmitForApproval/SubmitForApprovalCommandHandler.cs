using StayHub.Services.Hotel.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Hotel.Application.Features.SubmitForApproval;

/// <summary>
/// Handles submitting a hotel for admin approval.
/// Validates ownership, then delegates the state transition to the domain entity.
/// The domain enforces that only Draft or Rejected hotels can be submitted.
/// </summary>
public sealed class SubmitForApprovalCommandHandler : ICommandHandler<SubmitForApprovalCommand>
{
    private readonly IHotelRepository _hotelRepository;

    public SubmitForApprovalCommandHandler(IHotelRepository hotelRepository)
    {
        _hotelRepository = hotelRepository;
    }

    public async Task<Result> Handle(
        SubmitForApprovalCommand request,
        CancellationToken cancellationToken)
    {
        var hotel = await _hotelRepository.GetByIdAsync(
            request.HotelId, cancellationToken);

        if (hotel is null)
            return Result.Failure(HotelErrors.Hotel.NotFound);

        if (hotel.OwnerId != request.OwnerId)
            return Result.Failure(HotelErrors.Hotel.NotOwner);

        try
        {
            hotel.SubmitForApproval();
        }
        catch (InvalidOperationException)
        {
            return Result.Failure(HotelErrors.Hotel.InvalidStatusTransition);
        }

        _hotelRepository.Update(hotel);

        return Result.Success();
    }
}
