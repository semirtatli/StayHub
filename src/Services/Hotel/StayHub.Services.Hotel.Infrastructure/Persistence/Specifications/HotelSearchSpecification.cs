using StayHub.Services.Hotel.Domain.Entities;
using StayHub.Services.Hotel.Domain.Enums;
using StayHub.Services.Hotel.Domain.SearchCriteria;
using StayHub.Shared.Infrastructure.Specifications;

namespace StayHub.Services.Hotel.Infrastructure.Persistence.Specifications;

/// <summary>
/// Translates <see cref="HotelSearchCriteria"/> into an EF Core specification.
///
/// Builds a single WHERE expression with short-circuit null checks:
///   SQL Server evaluates (@param IS NULL OR Column op @param) efficiently —
///   the optimizer collapses no-op branches at plan compilation time.
///
/// Geo-distance uses a bounding-box pre-filter (±ΔLat/ΔLng calculated from
/// the search radius). The bounding box over-selects by up to ~27 % near the
/// corners; the caller trims to exact Haversine distance after materialization.
/// </summary>
public sealed class HotelSearchSpecification : Specification<HotelEntity>
{
    public HotelSearchSpecification(HotelSearchCriteria criteria)
    {
        // ── Capture filter values as locals for expression-tree closure ──
        var searchTerm = criteria.SearchTerm?.Trim();
        var city = criteria.City?.Trim();
        var country = criteria.Country?.Trim();
        var minStarRating = criteria.MinStarRating;
        var maxStarRating = criteria.MaxStarRating;
        var minPrice = criteria.MinPrice;
        var maxPrice = criteria.MaxPrice;
        var roomType = criteria.RoomType;

        // ── Geo bounding box ────────────────────────────────────────────
        double? minLat = null, maxLat = null, minLng = null, maxLng = null;
        var hasGeoFilter = criteria.Latitude.HasValue
                        && criteria.Longitude.HasValue
                        && criteria.RadiusKm.HasValue;

        if (hasGeoFilter)
        {
            CalculateBoundingBox(
                criteria.Latitude!.Value,
                criteria.Longitude!.Value,
                criteria.RadiusKm!.Value,
                out minLat, out maxLat, out minLng, out maxLng);
        }

        // ── Criteria ────────────────────────────────────────────────────
        Criteria = h =>
            // Only Active hotels appear in public search
            h.Status == HotelStatus.Active &&

            // Full-text search on name / description
            (searchTerm == null ||
                h.Name.Contains(searchTerm) ||
                h.Description.Contains(searchTerm)) &&

            // City filter (exact, case-insensitive via SQL collation)
            (city == null || h.Address.City == city) &&

            // Country filter
            (country == null || h.Address.Country == country) &&

            // Star-rating range
            (!minStarRating.HasValue || h.StarRating >= minStarRating.Value) &&
            (!maxStarRating.HasValue || h.StarRating <= maxStarRating.Value) &&

            // Price range — hotel has at least one active room in range
            (!minPrice.HasValue ||
                h.Rooms.Any(r => r.IsActive && r.BasePrice.Amount >= minPrice.Value)) &&
            (!maxPrice.HasValue ||
                h.Rooms.Any(r => r.IsActive && r.BasePrice.Amount <= maxPrice.Value)) &&

            // Room type filter
            (!roomType.HasValue ||
                h.Rooms.Any(r => r.IsActive && r.RoomType == roomType.Value)) &&

            // Geo bounding box
            (!hasGeoFilter ||
                (h.Location != null &&
                 h.Location.Latitude >= minLat!.Value &&
                 h.Location.Latitude <= maxLat!.Value &&
                 h.Location.Longitude >= minLng!.Value &&
                 h.Location.Longitude <= maxLng!.Value));

        // ── Includes ────────────────────────────────────────────────────
        AddInclude(h => h.Rooms);

        // ── Ordering ────────────────────────────────────────────────────
        ApplySorting(criteria.SortBy, criteria.SortDescending);

        // ── Performance ─────────────────────────────────────────────────
        ApplyNoTracking();
        ApplySplitQuery();
    }

    // ── Sorting ─────────────────────────────────────────────────────────

    private void ApplySorting(string? sortBy, bool descending)
    {
        switch (sortBy?.ToLowerInvariant())
        {
            case "name":
                if (descending) ApplyOrderByDescending(h => h.Name);
                else ApplyOrderBy(h => h.Name);
                break;

            case "starrating":
                if (descending) ApplyOrderByDescending(h => h.StarRating);
                else ApplyOrderBy(h => h.StarRating);
                break;

            case "price":
                // Sort by minimum active-room price.
                // DefaultIfEmpty(decimal.MaxValue) ensures hotels with no rooms sort last.
                if (descending)
                    ApplyOrderByDescending(h =>
                        h.Rooms.Where(r => r.IsActive)
                               .Select(r => r.BasePrice.Amount)
                               .DefaultIfEmpty(0m)
                               .Min());
                else
                    ApplyOrderBy(h =>
                        h.Rooms.Where(r => r.IsActive)
                               .Select(r => r.BasePrice.Amount)
                               .DefaultIfEmpty(decimal.MaxValue)
                               .Min());
                break;

            case "createdat":
                if (descending) ApplyOrderByDescending(h => h.CreatedAt);
                else ApplyOrderBy(h => h.CreatedAt);
                break;

            default:
                // Default: newest first
                ApplyOrderByDescending(h => h.CreatedAt);
                break;
        }
    }

    // ── Bounding-box helper ─────────────────────────────────────────────

    /// <summary>
    /// Calculates a lat/lng bounding box for a circle defined by center + radius.
    /// Uses spherical-earth approximation (1° latitude ≈ 111.32 km).
    /// </summary>
    private static void CalculateBoundingBox(
        double centerLat,
        double centerLng,
        double radiusKm,
        out double? minLat,
        out double? maxLat,
        out double? minLng,
        out double? maxLng)
    {
        const double kmPerDegreeLat = 111.32;

        var deltaLat = radiusKm / kmPerDegreeLat;
        var deltaLng = radiusKm / (kmPerDegreeLat * Math.Cos(centerLat * Math.PI / 180.0));

        minLat = centerLat - deltaLat;
        maxLat = centerLat + deltaLat;
        minLng = centerLng - deltaLng;
        maxLng = centerLng + deltaLng;
    }
}
