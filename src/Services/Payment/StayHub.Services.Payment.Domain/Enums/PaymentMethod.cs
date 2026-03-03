namespace StayHub.Services.Payment.Domain.Enums;

/// <summary>
/// Supported payment methods.
/// Extensible via Strategy pattern — each method has its own provider implementation.
/// </summary>
public enum PaymentMethod
{
    CreditCard = 0,
    DebitCard = 1,
    BankTransfer = 2
}
