using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using MarketPlaceApi.Services;

namespace MarketPlaceApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        public class CreatePaymentRequest
        {
            public long AmountInMinorUnit { get; set; }   // e.g. 1299 = £12.99
            public string Currency { get; set; } = "gbp";
            public string CustomerEmail { get; set; } = string.Empty;
        }

        [HttpPost("create-intent")]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] CreatePaymentRequest request)
        {
            var intent = await _paymentService.CreatePaymentIntentAsync(
                request.AmountInMinorUnit,
                request.Currency,
                request.CustomerEmail
            );

            // client_secret is what the frontend uses with Stripe.js
            return Ok(new
            {
                clientSecret = intent.ClientSecret
            });
        }

        // Optional: webhook endpoint
        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            var json = await new System.IO.StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var stripeSignature = Request.Headers["Stripe-Signature"];

            await _paymentService.HandleWebhookAsync(json, stripeSignature!);

            return Ok();
        }
    }
}