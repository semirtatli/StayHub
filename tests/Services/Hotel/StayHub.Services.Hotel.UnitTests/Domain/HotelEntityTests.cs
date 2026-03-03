using FluentAssertions;
using StayHub.Services.Hotel.Domain.Entities;
using StayHub.Services.Hotel.Domain.Enums;
using StayHub.Services.Hotel.Domain.ValueObjects;

namespace StayHub.Services.Hotel.UnitTests.Domain;

public class HotelEntityTests
{
    private static Address CreateTestAddress() =>
        Address.Create("123 Main St", "Istanbul", "Istanbul", "Turkey", "34000");

    private static ContactInfo CreateTestContact() =>
        ContactInfo.Create("+90-555-1234567", "test@hotel.com");

    // ── Factory ─────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidParams_ShouldCreateHotelInDraftStatus()
    {
        var hotel = HotelEntity.Create(
            "Grand Hotel",
            "A luxurious hotel in the heart of Istanbul",
            5,
            CreateTestAddress(),
            CreateTestContact(),
            "owner-1");

        hotel.Name.Should().Be("Grand Hotel");
        hotel.StarRating.Should().Be(5);
        hotel.Status.Should().Be(HotelStatus.Draft);
        hotel.OwnerId.Should().Be("owner-1");
        hotel.CheckInTime.Should().Be(new TimeOnly(14, 0));
        hotel.CheckOutTime.Should().Be(new TimeOnly(11, 0));
        hotel.DomainEvents.Should().ContainSingle();
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrow()
    {
        var act = () => HotelEntity.Create("", "desc", 3, CreateTestAddress(), CreateTestContact(), "owner-1");

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void Create_WithInvalidStarRating_ShouldThrow(int rating)
    {
        var act = () => HotelEntity.Create("Hotel", "desc", rating, CreateTestAddress(), CreateTestContact(), "owner-1");

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Create_WithNullAddress_ShouldThrow()
    {
        var act = () => HotelEntity.Create("Hotel", "desc", 3, null!, CreateTestContact(), "owner-1");

        act.Should().Throw<ArgumentNullException>();
    }

    // ── Status Workflow ─────────────────────────────────────────────────

    [Fact]
    public void SubmitForApproval_FromDraft_ShouldTransitionToPendingApproval()
    {
        var hotel = HotelEntity.Create("Hotel", "desc", 3, CreateTestAddress(), CreateTestContact(), "owner-1");

        hotel.SubmitForApproval();

        hotel.Status.Should().Be(HotelStatus.PendingApproval);
    }

    [Fact]
    public void Approve_FromPendingApproval_ShouldTransitionToActive()
    {
        var hotel = HotelEntity.Create("Hotel", "desc", 3, CreateTestAddress(), CreateTestContact(), "owner-1");
        hotel.SubmitForApproval();

        hotel.Approve("admin-1");

        hotel.Status.Should().Be(HotelStatus.Active);
    }

    [Fact]
    public void Approve_FromDraft_ShouldThrow()
    {
        var hotel = HotelEntity.Create("Hotel", "desc", 3, CreateTestAddress(), CreateTestContact(), "owner-1");

        var act = () => hotel.Approve("admin-1");

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Reject_FromPendingApproval_ShouldTransitionToRejected()
    {
        var hotel = HotelEntity.Create("Hotel", "desc", 3, CreateTestAddress(), CreateTestContact(), "owner-1");
        hotel.SubmitForApproval();

        hotel.Reject("admin-1", "Missing photos");

        hotel.Status.Should().Be(HotelStatus.Rejected);
        hotel.StatusReason.Should().Be("Missing photos");
    }

    [Fact]
    public void Reject_WithoutReason_ShouldThrow()
    {
        var hotel = HotelEntity.Create("Hotel", "desc", 3, CreateTestAddress(), CreateTestContact(), "owner-1");
        hotel.SubmitForApproval();

        var act = () => hotel.Reject("admin-1", "");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Suspend_FromActive_ShouldTransitionToSuspended()
    {
        var hotel = HotelEntity.Create("Hotel", "desc", 3, CreateTestAddress(), CreateTestContact(), "owner-1");
        hotel.SubmitForApproval();
        hotel.Approve("admin-1");

        hotel.Suspend("admin-1", "Policy violation");

        hotel.Status.Should().Be(HotelStatus.Suspended);
    }

    [Fact]
    public void Reactivate_FromSuspended_ShouldTransitionToActive()
    {
        var hotel = HotelEntity.Create("Hotel", "desc", 3, CreateTestAddress(), CreateTestContact(), "owner-1");
        hotel.SubmitForApproval();
        hotel.Approve("admin-1");
        hotel.Suspend("admin-1");

        hotel.Reactivate("admin-1");

        hotel.Status.Should().Be(HotelStatus.Active);
    }

    [Fact]
    public void SubmitForApproval_FromRejected_ShouldResubmit()
    {
        var hotel = HotelEntity.Create("Hotel", "desc", 3, CreateTestAddress(), CreateTestContact(), "owner-1");
        hotel.SubmitForApproval();
        hotel.Reject("admin-1", "Needs fixes");

        hotel.SubmitForApproval();

        hotel.Status.Should().Be(HotelStatus.PendingApproval);
    }

    [Fact]
    public void SubmitForApproval_FromActive_ShouldThrow()
    {
        var hotel = HotelEntity.Create("Hotel", "desc", 3, CreateTestAddress(), CreateTestContact(), "owner-1");
        hotel.SubmitForApproval();
        hotel.Approve("admin-1");

        var act = () => hotel.SubmitForApproval();

        act.Should().Throw<InvalidOperationException>();
    }

    // ── Room Management ─────────────────────────────────────────────────

    [Fact]
    public void AddRoom_ShouldAddRoomToHotel()
    {
        var hotel = HotelEntity.Create("Hotel", "desc", 3, CreateTestAddress(), CreateTestContact(), "owner-1");
        var basePrice = Money.Create(100m, "USD");

        var room = hotel.AddRoom("Deluxe Room", "A deluxe room", RoomType.Double, 2, basePrice, 10);

        hotel.Rooms.Should().ContainSingle();
        room.Name.Should().Be("Deluxe Room");
        room.HotelId.Should().Be(hotel.Id);
    }

    [Fact]
    public void AddRoom_WithDuplicateName_ShouldThrow()
    {
        var hotel = HotelEntity.Create("Hotel", "desc", 3, CreateTestAddress(), CreateTestContact(), "owner-1");
        var basePrice = Money.Create(100m, "USD");
        hotel.AddRoom("Deluxe Room", "A deluxe room", RoomType.Double, 2, basePrice, 10);

        var act = () => hotel.AddRoom("Deluxe Room", "Another deluxe room", RoomType.Double, 2, basePrice, 5);

        act.Should().Throw<InvalidOperationException>().WithMessage("*already exists*");
    }

    [Fact]
    public void RemoveRoom_ShouldRemoveRoomFromHotel()
    {
        var hotel = HotelEntity.Create("Hotel", "desc", 3, CreateTestAddress(), CreateTestContact(), "owner-1");
        var room = hotel.AddRoom("Deluxe", "desc", RoomType.Double, 2, Money.Create(100m, "USD"), 10);

        hotel.RemoveRoom(room.Id);

        hotel.Rooms.Should().BeEmpty();
    }

    [Fact]
    public void RemoveRoom_WithInvalidId_ShouldThrow()
    {
        var hotel = HotelEntity.Create("Hotel", "desc", 3, CreateTestAddress(), CreateTestContact(), "owner-1");

        var act = () => hotel.RemoveRoom(Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>();
    }

    // ── Update ──────────────────────────────────────────────────────────

    [Fact]
    public void Update_InDraftStatus_ShouldUpdateFields()
    {
        var hotel = HotelEntity.Create("Hotel", "desc", 3, CreateTestAddress(), CreateTestContact(), "owner-1");
        var newAddress = Address.Create("456 New St", "Ankara", "Ankara", "Turkey", "06000");
        var newContact = ContactInfo.Create("+90-555-9999999", "new@hotel.com");

        hotel.Update("Updated Hotel", "New description", 4, newAddress, newContact, new TimeOnly(15, 0), new TimeOnly(12, 0));

        hotel.Name.Should().Be("Updated Hotel");
        hotel.StarRating.Should().Be(4);
        hotel.Address.City.Should().Be("Ankara");
    }

    [Fact]
    public void Update_InPendingApprovalStatus_ShouldThrow()
    {
        var hotel = HotelEntity.Create("Hotel", "desc", 3, CreateTestAddress(), CreateTestContact(), "owner-1");
        hotel.SubmitForApproval();

        var act = () => hotel.Update("New Name", "desc", 3, CreateTestAddress(), CreateTestContact(), new TimeOnly(14, 0), new TimeOnly(11, 0));

        act.Should().Throw<InvalidOperationException>();
    }

    // ── Photo Management ────────────────────────────────────────────────

    [Fact]
    public void AddPhotoUrl_ShouldAddPhoto()
    {
        var hotel = HotelEntity.Create("Hotel", "desc", 3, CreateTestAddress(), CreateTestContact(), "owner-1");

        hotel.AddPhotoUrl("https://cdn.example.com/photo1.jpg");

        hotel.PhotoUrls.Should().ContainSingle().Which.Should().Be("https://cdn.example.com/photo1.jpg");
    }

    [Fact]
    public void AddPhotoUrl_Duplicate_ShouldNotAddAgain()
    {
        var hotel = HotelEntity.Create("Hotel", "desc", 3, CreateTestAddress(), CreateTestContact(), "owner-1");

        hotel.AddPhotoUrl("https://cdn.example.com/photo1.jpg");
        hotel.AddPhotoUrl("https://cdn.example.com/photo1.jpg");

        hotel.PhotoUrls.Should().HaveCount(1);
    }
}
