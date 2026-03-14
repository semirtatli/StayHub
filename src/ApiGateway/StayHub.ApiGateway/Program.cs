using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using StayHub.ApiGateway.Middleware;

// =====================================================
// StayHub API Gateway — Program.cs
// =====================================================
// Single entry point for all client traffic.
// Routes requests to backend microservices via YARP.
//
// Pipeline order:
// 1. Serilog request logging
// 2. Global exception handler (catches gateway errors)
// 3. Request logging middleware (correlation ID, timing)
// 4. CORS
// 5. Rate limiting
// 6. Authentication forwarding (headers pass through)
// 7. YARP reverse proxy
// =====================================================

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ──────────────────────────────────────────
builder.Host.UseSerilog((context, loggerConfig) =>
{
    loggerConfig.ReadFrom.Configuration(context.Configuration);
});

// ── YARP Reverse Proxy ───────────────────────────────
// Load route/cluster config from yarp.json (separate from appsettings for clarity)
// Environment-specific override (yarp.Development.json) uses localhost addresses for local dev.
builder.Configuration.AddJsonFile("yarp.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile($"yarp.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// ── CORS ─────────────────────────────────────────────
// React frontend (dev: localhost:3000/5173) needs cross-origin access
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()  // Required for httpOnly cookie (refresh token)
            .WithExposedHeaders("X-Correlation-Id", "X-Pagination");
    });
});

// ── Rate Limiting ────────────────────────────────────
// Protects backend services from excessive traffic.
// Two policies:
// - "fixed": General API rate limit (e.g., 100 req/min per IP)
// - "sliding": Stricter limit for sensitive endpoints (e.g., login, register)
builder.Services.AddRateLimiter(options =>
{
    var fixedConfig = builder.Configuration.GetSection("RateLimiting:FixedWindow");
    var slidingConfig = builder.Configuration.GetSection("RateLimiting:SlidingWindow");

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // General rate limit — applied to all proxied routes by default
    options.AddFixedWindowLimiter("fixed", limiterOptions =>
    {
        limiterOptions.PermitLimit = fixedConfig.GetValue("PermitLimit", 100);
        limiterOptions.Window = TimeSpan.FromSeconds(fixedConfig.GetValue("WindowSeconds", 60));
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 5;
    });

    // Stricter rate limit — for auth endpoints (login, register, password reset)
    options.AddSlidingWindowLimiter("sliding", limiterOptions =>
    {
        limiterOptions.PermitLimit = slidingConfig.GetValue("PermitLimit", 30);
        limiterOptions.Window = TimeSpan.FromSeconds(slidingConfig.GetValue("WindowSeconds", 60));
        limiterOptions.SegmentsPerWindow = slidingConfig.GetValue("SegmentsPerWindow", 6);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 2;
    });

    // Partition by client IP address
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";

        return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = fixedConfig.GetValue("PermitLimit", 100),
            Window = TimeSpan.FromSeconds(fixedConfig.GetValue("WindowSeconds", 60))
        });
    });
});

// ── Health Checks ────────────────────────────────────
builder.Services.AddHealthChecks();

var app = builder.Build();

// ── Middleware Pipeline ──────────────────────────────
// Order matters: outermost → innermost

// Global exception handler (must be first to catch all errors)
app.UseMiddleware<GlobalExceptionMiddleware>();

// Request logging with correlation ID and timing
app.UseMiddleware<RequestLoggingMiddleware>();

// CORS must be before routing
app.UseCors();

// Rate limiting
app.UseRateLimiter();

// Health endpoint for load balancer / container orchestrator
app.MapHealthChecks("/health");

// Gateway status endpoint
app.MapGet("/", () => Results.Ok(new
{
    service = "StayHub API Gateway",
    status = "running",
    timestamp = DateTime.UtcNow
}));

// YARP reverse proxy — maps all routes to backend services
app.MapReverseProxy();

// ── Start ────────────────────────────────────────────
Log.Information("StayHub API Gateway starting on port 5000...");
app.Run();
