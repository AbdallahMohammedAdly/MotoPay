using AutoLease.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;

namespace AutoLease.Infrastructure.Services;

public class StripePaymentService : IPaymentService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripePaymentService> _logger;

    public StripePaymentService(IConfiguration configuration, ILogger<StripePaymentService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        // Set the Stripe API key
        StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
    }

    public async Task<string> CreatePaymentSessionAsync(decimal amount, string currency, string successUrl, 
        string cancelUrl, Dictionary<string, string>? metadata = null)
    {
        _logger.LogInformation("Creating Stripe payment session for amount: {Amount} {Currency}", amount, currency);

        try
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(amount * 100), // Convert to cents
                            Currency = currency.ToLower(),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "AutoLease Payment",
                                Description = "Payment for AutoLease services"
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                Metadata = metadata ?? new Dictionary<string, string>()
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            _logger.LogInformation("Successfully created Stripe session with ID: {SessionId}", session.Id);

            return session.Id;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error creating Stripe payment session: {Message}", ex.Message);
            throw new InvalidOperationException($"Failed to create payment session: {ex.Message}", ex);
        }
    }

    public async Task<bool> ValidatePaymentAsync(string sessionId)
    {
        _logger.LogInformation("Validating payment for session: {SessionId}", sessionId);

        try
        {
            var service = new SessionService();
            var session = await service.GetAsync(sessionId);

            var isValid = session.PaymentStatus == "paid";
            
            _logger.LogInformation("Payment validation result for session {SessionId}: {IsValid}", sessionId, isValid);

            return isValid;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error validating payment for session {SessionId}: {Message}", sessionId, ex.Message);
            return false;
        }
    }

    public async Task<string> GetPaymentStatusAsync(string sessionId)
    {
        _logger.LogInformation("Getting payment status for session: {SessionId}", sessionId);

        try
        {
            var service = new SessionService();
            var session = await service.GetAsync(sessionId);

            _logger.LogInformation("Payment status for session {SessionId}: {Status}", sessionId, session.PaymentStatus);

            return session.PaymentStatus ?? "unknown";
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error getting payment status for session {SessionId}: {Message}", sessionId, ex.Message);
            return "error";
        }
    }
}