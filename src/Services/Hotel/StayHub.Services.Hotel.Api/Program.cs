using Microsoft.EntityFrameworkCore;
using Serilog;
using StayHub.Services.Hotel.Api.Middleware;
using StayHub.Services.Hotel.Application;
using StayHub.Services.Hotel.Infrastructure;
using StayHub.Services.Hotel.Infrastructure.Persistence;
using StayHub.Shared.Web;
using StayHub.Shared.Web.Versioning;
using StayHub.Shared.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ──────────────────────────────────────────────────────────────
builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

// ── Application & Infrastructure layers ──────────────────────────────────
builder.Services.AddHotelApplication();
builder.Services.AddHotelInfrastructure(builder.Configuration);
builder.Services.AddSharedWebServices();

// ── Authentication (JWT Bearer — validates tokens issued by Identity Service) ──
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "Bearer";
        options.DefaultChallengeScheme = "Bearer";
    })
    .AddJwtBearer("Bearer", options =>
    {
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(jwtSettings["Secret"]!)),
            ClockSkew = TimeSpan.Zero
        };
    });

// ── Authorization policies ───────────────────────────────────────────────
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("HotelOwnerOrAdmin", policy =>
        policy.RequireRole("HotelOwner", "Admin"));

    options.AddPolicy("Authenticated", policy =>
        policy.RequireAuthenticatedUser());
});

// ── Controllers + Swagger ────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "StayHub Hotel API",
        Version = "v1",
        Description = "Hotel management, room management, and search for StayHub."
    });

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT token"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ── API Versioning ──────────────────────────────────────────────────────
builder.Services.AddStayHubApiVersioning();

// ── Health checks ────────────────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddDbContextCheck<HotelDbContext>("hotel-db");

// ── CORS (dev) ───────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy
            .WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? ["http://localhost:3000"])
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

var app = builder.Build();

// ── Database migration (development only) ────────────────────────────────
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

// ── Middleware pipeline ──────────────────────────────────────────────────
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// Serve uploaded photos from wwwroot/uploads/
app.UseStaticFiles();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
