using Microsoft.Extensions.Configuration;
using Stripe;

namespace MarketPlaceApi.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly PaymentIntentService _paymentIntentService;
        private readonly IConfiguration _configuration;

        public PaymentService(IConfiguration configuration)
        {
            _configuration = configuration;

            // You can also set this globally in Program.cs
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];

            _paymentIntentService = new PaymentIntentService();
        }
        public async Task<PaymentIntent> CreatePaymentIntentAsync(long ammountInMinorUnit, string currency, string customerEmail)
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = ammountInMinorUnit,
                Currency = currency,     // "gbp", "usd", etc.
                ReceiptEmail = customerEmail,
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true
                }
            };

            var PaymentIntent = await _paymentIntentService.CreateAsync(options);
            return PaymentIntent;
        }

        public Task HandleWebhookAsync(string json, string stripeSignature)
        {
            // In a real app:
            //  - use EventUtility.ConstructEvent(json, stripeSignature, webhookSecret)
            //  - switch on event.Type
            //  - update order status in DB, etc.
            return Task.CompletedTask;
        }
    }
}