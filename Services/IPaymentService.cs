using Stripe;

namespace MarketPlaceApi.Services
{
    public interface IPaymentService
    {
        // For basic testing: create a PaymentIntent
        Task<PaymentIntent> CreatePaymentIntentAsync(long ammountInMinorUnit, string currency, string customerEmail);

        // Optional: if you’re planning webhooks soon
        Task HandleWebhookAsync(string json, string stripeSignature);
    }
}