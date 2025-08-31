namespace AutoLease.Application.Interfaces;

public interface IPaymentService
{
    Task<string> CreatePaymentSessionAsync(decimal amount, string currency, string successUrl, string cancelUrl, Dictionary<string, string>? metadata = null);
    Task<bool> ValidatePaymentAsync(string sessionId);
    Task<string> GetPaymentStatusAsync(string sessionId);
}