using Asp.Versioning;
using Microsoft.Extensions.DependencyInjection;

namespace StayHub.Shared.Web.Versioning;

/// <summary>
/// Extension methods for adding API versioning to services.
/// All StayHub services start at v1.0 using URL segment versioning.
/// </summary>
public static class ApiVersioningExtensions
{
    /// <summary>
    /// Registers API versioning with a default version of 1.0.
    /// Supports URL segment (api/v{version}/...) and query-string (?api-version=1.0).
    /// </summary>
    public static IServiceCollection AddStayHubApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new QueryStringApiVersionReader("api-version"),
                new HeaderApiVersionReader("X-Api-Version"));
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }
}
