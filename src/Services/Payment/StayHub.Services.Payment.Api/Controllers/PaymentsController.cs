using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayHub.Services.Payment.Application.Features.CancelPayment;
using StayHub.Services.Payment.Application.Features.CreatePayment;
using StayHub.Services.Payment.Application.Features.GetPaymentById;
using StayHub.Services.Payment.Application.Features.GetPaymentsByBooking;
using StayHub.Services.Payment.Application.Features.ProcessWebhook;
using StayHub.Services.Payment.Application.Features.RefundPayment;
using StayHub.Services.Payment.Domain.Enums;

namespace StayHub.Services.Payment.Api.Controllers;

/// <summary>
/// Payment management endpoints — create payments, process webhooks, refunds, queries.
/// </summary>
public sealed class PaymentsController : ApiController
{
    // ── Commands ──────────────────────────────────────────────────────────

    /// <summary>
    /// Initiates a payment for a booking. Creates a Stripe PaymentIntent.
    /// Returns a client secret for the frontend to complete the payment.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "Authenticated")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var command = new CreatePaymentCommand(
            request.BookingId,
            request.Amount,
            request.Currency,
            request.Method,
            userId);

        var result = await Mediator.Send(command);

        return HandleCreatedResult(result, nameof(GetPaymentById), new { id = result.IsSuccess ? result.Value.PaymentId : Guid.Empty });
    }

    /// <summary>
    /// Stripe webhook endpoint — processes payment status updates.
    /// No authentication required — verified by webhook signature.
    /// </summary>
    [HttpPost("webhook")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ProcessWebhook()
    {
        using var reader = new StreamReader(HttpContext.Request.Body);
        var payload = await reader.ReadToEndAsync();
        var signature = HttpContext.Request.Headers["Stripe-Signature"].ToString();

        var command = new ProcessWebhookCommand(payload, signature);
        var result = await Mediator.Send(command);

        return HandleResult(result);
    }

    /// <summary>
    /// Processes a refund (full or partial) for a succeeded payment.
    /// </summary>
    [HttpPost("{id:guid}/refund")]
    [Authorize(Policy = "Authenticated")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RefundPayment(Guid id, [FromBody] RefundPaymentRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var command = new RefundPaymentCommand(id, request.Amount, userId);
        var result = await Mediator.Send(command);

        return HandleResult(result);
    }

    /// <summary>
    /// Cancels a pending payment.
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = "Authenticated")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelPayment(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var command = new CancelPaymentCommand(id, userId);
        var result = await Mediator.Send(command);

        return HandleResult(result);
    }

    // ── Queries ──────────────────────────────────────────────────────────

    /// <summary>
    /// Gets a payment by its ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "Authenticated")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPaymentById(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var query = new GetPaymentByIdQuery(id, userId);
        var result = await Mediator.Send(query);

        return HandleResult(result);
    }

    /// <summary>
    /// Gets all payments for a specific booking.
    /// </summary>
    [HttpGet("booking/{bookingId:guid}")]
    [Authorize(Policy = "Authenticated")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaymentsByBooking(Guid bookingId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var query = new GetPaymentsByBookingQuery(bookingId, userId);
        var result = await Mediator.Send(query);

        return HandleResult(result);
    }

    /// <summary>
    /// Health check endpoint.
    /// </summary>
    [HttpGet("ping")]
    [AllowAnonymous]
    public IActionResult Ping() => Ok(new { service = "StayHub.Payment", status = "healthy" });
}

// ── Request DTOs (API-layer only — not shared) ──────────────────────────

/// <summary>
/// Request body for creating a payment.
/// </summary>
public sealed record CreatePaymentRequest(
    Guid BookingId,
    decimal Amount,
    string Currency,
    PaymentMethod Method);

/// <summary>
/// Request body for processing a refund.
/// </summary>
public sealed record RefundPaymentRequest(decimal Amount);
