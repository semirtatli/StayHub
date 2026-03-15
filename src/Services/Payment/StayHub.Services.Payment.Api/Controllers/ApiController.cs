using StayHub.Shared.Web.Controllers;

namespace StayHub.Services.Payment.Api.Controllers;

/// <summary>
/// Base controller for Payment API endpoints.
/// Inherits shared Result-to-HTTP mapping from ApiControllerBase.
/// </summary>
public abstract class ApiController : ApiControllerBase;
