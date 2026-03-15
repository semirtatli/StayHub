using FluentAssertions;
using StayHub.Services.Review.Domain.Entities;
using StayHub.Services.Review.Domain.ValueObjects;

namespace StayHub.Services.Review.UnitTests.Domain;

public class ReviewEntityTests
{
    private static Rating CreateTestRating(
        int cleanliness = 4, int service = 5, int location = 3,
        int comfort = 4, int valueForMoney = 4) =>
        Rating.Create(cleanliness, service, location, comfort, valueForMoney);

    // ── Factory ─────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidParams_ShouldCreateReview()
    {
        var rating = CreateTestRating();

        var review = ReviewEntity.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "user-1",
            "John Doe",
            "Great Stay",
            "Had a wonderful time at this hotel. Highly recommend!",
            rating,
            DateOnly.FromDateTime(DateTime.Today.AddDays(-7)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(-5)));

        review.UserId.Should().Be("user-1");
        review.GuestName.Should().Be("John Doe");
        review.Title.Should().Be("Great Stay");
        review.Body.Should().Be("Had a wonderful time at this hotel. Highly recommend!");
        review.Rating.Should().Be(rating);
        review.Rating.Overall.Should().Be(4.0m);
        review.ManagementResponse.Should().BeNull();
        review.ManagementResponseAt.Should().BeNull();
        review.IsDeleted.Should().BeFalse();
        review.DomainEvents.Should().ContainSingle();
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldThrow()
    {
        var act = () => ReviewEntity.Create(
            Guid.NewGuid(), Guid.NewGuid(), "", "John Doe",
            "Great Stay", "Had a wonderful time at this hotel. Highly recommend!",
            CreateTestRating(),
            DateOnly.FromDateTime(DateTime.Today.AddDays(-7)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(-5)));

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyGuestName_ShouldThrow()
    {
        var act = () => ReviewEntity.Create(
            Guid.NewGuid(), Guid.NewGuid(), "user-1", "",
            "Great Stay", "Had a wonderful time at this hotel. Highly recommend!",
            CreateTestRating(),
            DateOnly.FromDateTime(DateTime.Today.AddDays(-7)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(-5)));

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithTitleTooShort_ShouldThrow()
    {
        var act = () => ReviewEntity.Create(
            Guid.NewGuid(), Guid.NewGuid(), "user-1", "John Doe",
            "Hi", "Had a wonderful time at this hotel. Highly recommend!",
            CreateTestRating(),
            DateOnly.FromDateTime(DateTime.Today.AddDays(-7)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(-5)));

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Create_WithTitleTooLong_ShouldThrow()
    {
        var longTitle = new string('A', 201);

        var act = () => ReviewEntity.Create(
            Guid.NewGuid(), Guid.NewGuid(), "user-1", "John Doe",
            longTitle, "Had a wonderful time at this hotel. Highly recommend!",
            CreateTestRating(),
            DateOnly.FromDateTime(DateTime.Today.AddDays(-7)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(-5)));

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Create_WithBodyTooShort_ShouldThrow()
    {
        var act = () => ReviewEntity.Create(
            Guid.NewGuid(), Guid.NewGuid(), "user-1", "John Doe",
            "Great Stay", "Too short",
            CreateTestRating(),
            DateOnly.FromDateTime(DateTime.Today.AddDays(-7)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(-5)));

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Create_WithBodyTooLong_ShouldThrow()
    {
        var longBody = new string('A', 5001);

        var act = () => ReviewEntity.Create(
            Guid.NewGuid(), Guid.NewGuid(), "user-1", "John Doe",
            "Great Stay", longBody,
            CreateTestRating(),
            DateOnly.FromDateTime(DateTime.Today.AddDays(-7)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(-5)));

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ── Update ──────────────────────────────────────────────────────────

    [Fact]
    public void Update_WithValidParams_ShouldUpdateReview()
    {
        var review = CreateTestReview();
        var newRating = CreateTestRating(5, 5, 5, 5, 5);

        review.Update("Updated Title", "Updated body text that is long enough to pass validation.", newRating);

        review.Title.Should().Be("Updated Title");
        review.Body.Should().Be("Updated body text that is long enough to pass validation.");
        review.Rating.Should().Be(newRating);
        review.Rating.Overall.Should().Be(5.0m);
    }

    [Fact]
    public void Update_WithTitleTooShort_ShouldThrow()
    {
        var review = CreateTestReview();

        var act = () => review.Update("Hi", "Updated body text that is long enough.", CreateTestRating());

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Update_WithBodyTooShort_ShouldThrow()
    {
        var review = CreateTestReview();

        var act = () => review.Update("Valid Title", "Too short", CreateTestRating());

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ── Management Response ─────────────────────────────────────────────

    [Fact]
    public void AddManagementResponse_WithValidResponse_ShouldSetResponse()
    {
        var review = CreateTestReview();

        review.AddManagementResponse("Thank you for your review! We are glad you enjoyed your stay.");

        review.ManagementResponse.Should().Be("Thank you for your review! We are glad you enjoyed your stay.");
        review.ManagementResponseAt.Should().NotBeNull();
    }

    [Fact]
    public void AddManagementResponse_WithEmptyResponse_ShouldThrow()
    {
        var review = CreateTestReview();

        var act = () => review.AddManagementResponse("");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddManagementResponse_WithResponseTooLong_ShouldThrow()
    {
        var review = CreateTestReview();
        var longResponse = new string('A', 2001);

        var act = () => review.AddManagementResponse(longResponse);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void AddManagementResponse_CalledTwice_ShouldOverwriteResponse()
    {
        var review = CreateTestReview();

        review.AddManagementResponse("First response from management.");
        review.AddManagementResponse("Updated response from management.");

        review.ManagementResponse.Should().Be("Updated response from management.");
    }

    // ── Soft Delete ─────────────────────────────────────────────────────

    [Fact]
    public void SoftDelete_ShouldMarkAsDeleted()
    {
        var review = CreateTestReview();

        review.IsDeleted = true;
        review.DeletedAt = DateTime.UtcNow;
        review.DeletedBy = "admin-1";

        review.IsDeleted.Should().BeTrue();
        review.DeletedAt.Should().NotBeNull();
        review.DeletedBy.Should().Be("admin-1");
    }

    [Fact]
    public void SoftDelete_NewReview_ShouldNotBeDeleted()
    {
        var review = CreateTestReview();

        review.IsDeleted.Should().BeFalse();
        review.DeletedAt.Should().BeNull();
        review.DeletedBy.Should().BeNull();
    }

    // ── Rating Validation (1-5 range) ───────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void Rating_WithInvalidCleanliness_ShouldThrow(int score)
    {
        var act = () => Rating.Create(score, 3, 3, 3, 3);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void Rating_WithInvalidService_ShouldThrow(int score)
    {
        var act = () => Rating.Create(3, score, 3, 3, 3);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void Rating_WithInvalidLocation_ShouldThrow(int score)
    {
        var act = () => Rating.Create(3, 3, score, 3, 3);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void Rating_WithInvalidComfort_ShouldThrow(int score)
    {
        var act = () => Rating.Create(3, 3, 3, score, 3);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void Rating_WithInvalidValueForMoney_ShouldThrow(int score)
    {
        var act = () => Rating.Create(3, 3, 3, 3, score);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Rating_WithAllValidScores_ShouldCalculateOverall()
    {
        var rating = Rating.Create(5, 4, 3, 2, 1);

        rating.Overall.Should().Be(3.0m);
    }

    [Fact]
    public void Rating_WithAllMaxScores_ShouldReturnFive()
    {
        var rating = Rating.Create(5, 5, 5, 5, 5);

        rating.Overall.Should().Be(5.0m);
    }

    [Fact]
    public void Rating_WithAllMinScores_ShouldReturnOne()
    {
        var rating = Rating.Create(1, 1, 1, 1, 1);

        rating.Overall.Should().Be(1.0m);
    }

    // ── Cannot Update a Deleted Review ──────────────────────────────────
    // Note: The domain entity does not currently enforce this invariant.
    // This test documents that a deleted review can still be updated at the
    // entity level. The application layer is responsible for preventing this.

    [Fact]
    public void Update_OnDeletedReview_EntityDoesNotPrevent()
    {
        var review = CreateTestReview();
        review.IsDeleted = true;
        review.DeletedAt = DateTime.UtcNow;

        // The entity layer does not block updates on soft-deleted reviews;
        // this is enforced at the application/command handler level.
        var newRating = CreateTestRating(2, 2, 2, 2, 2);
        review.Update("Still Updates", "The body is long enough to pass the validation check.", newRating);

        review.Title.Should().Be("Still Updates");
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private static ReviewEntity CreateTestReview() =>
        ReviewEntity.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "user-1",
            "John Doe",
            "Great Stay",
            "Had a wonderful time at this hotel. Highly recommend!",
            CreateTestRating(),
            DateOnly.FromDateTime(DateTime.Today.AddDays(-7)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(-5)));
}
