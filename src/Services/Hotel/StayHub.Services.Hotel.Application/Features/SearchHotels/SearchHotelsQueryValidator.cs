using FluentValidation;
using StayHub.Services.Hotel.Domain.Enums;

namespace StayHub.Services.Hotel.Application.Features.SearchHotels;

/// <summary>
/// Validates search query parameters before hitting the database.
/// Prevents invalid ranges, out-of-bounds coordinates, and unreasonable page sizes.
/// </summary>
public sealed class SearchHotelsQueryValidator : AbstractValidator<SearchHotelsQuery>
{
    private static readonly string[] ValidSortFields = ["name", "starrating", "price", "createdat"];

    public SearchHotelsQueryValidator()
    {
        // ── Pagination ──────────────────────────────────────────────────
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100.");

        // ── Star rating ─────────────────────────────────────────────────
        RuleFor(x => x.MinStarRating)
            .InclusiveBetween(1, 5)
            .When(x => x.MinStarRating.HasValue)
            .WithMessage("MinStarRating must be between 1 and 5.");

        RuleFor(x => x.MaxStarRating)
            .InclusiveBetween(1, 5)
            .When(x => x.MaxStarRating.HasValue)
            .WithMessage("MaxStarRating must be between 1 and 5.");

        RuleFor(x => x.MaxStarRating)
            .GreaterThanOrEqualTo(x => x.MinStarRating)
            .When(x => x.MinStarRating.HasValue && x.MaxStarRating.HasValue)
            .WithMessage("MaxStarRating must be >= MinStarRating.");

        // ── Price range ─────────────────────────────────────────────────
        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinPrice.HasValue)
            .WithMessage("MinPrice must be >= 0.");

        RuleFor(x => x.MaxPrice)
            .GreaterThan(0)
            .When(x => x.MaxPrice.HasValue)
            .WithMessage("MaxPrice must be > 0.");

        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(x => x.MinPrice)
            .When(x => x.MinPrice.HasValue && x.MaxPrice.HasValue)
            .WithMessage("MaxPrice must be >= MinPrice.");

        // ── Geo coordinates ─────────────────────────────────────────────
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90)
            .When(x => x.Latitude.HasValue)
            .WithMessage("Latitude must be between -90 and 90.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180)
            .When(x => x.Longitude.HasValue)
            .WithMessage("Longitude must be between -180 and 180.");

        RuleFor(x => x.Longitude)
            .NotNull()
            .When(x => x.Latitude.HasValue)
            .WithMessage("Longitude is required when Latitude is provided.");

        RuleFor(x => x.Latitude)
            .NotNull()
            .When(x => x.Longitude.HasValue)
            .WithMessage("Latitude is required when Longitude is provided.");

        RuleFor(x => x.RadiusKm)
            .NotNull()
            .When(x => x.Latitude.HasValue && x.Longitude.HasValue)
            .WithMessage("RadiusKm is required when coordinates are provided.");

        RuleFor(x => x.RadiusKm)
            .GreaterThan(0).When(x => x.RadiusKm.HasValue)
            .LessThanOrEqualTo(500).When(x => x.RadiusKm.HasValue)
            .WithMessage("RadiusKm must be between 0 and 500.");

        // ── Room type ───────────────────────────────────────────────────
        RuleFor(x => x.RoomType)
            .Must(BeValidRoomType)
            .When(x => x.RoomType is not null)
            .WithMessage("Invalid room type. Valid values: " +
                string.Join(", ", Enum.GetNames<RoomType>()));

        // ── Sort ────────────────────────────────────────────────────────
        RuleFor(x => x.SortBy)
            .Must(BeValidSortField)
            .When(x => x.SortBy is not null)
            .WithMessage("SortBy must be one of: name, starRating, price, createdAt.");
    }

    private static bool BeValidRoomType(string? roomType) =>
        Enum.TryParse<RoomType>(roomType, ignoreCase: true, out _);

    private static bool BeValidSortField(string? sortBy) =>
        Array.Exists(ValidSortFields, f => f.Equals(sortBy, StringComparison.OrdinalIgnoreCase));
}
