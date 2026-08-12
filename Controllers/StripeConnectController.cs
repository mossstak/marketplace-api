using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using MarketPlaceApi.Dtos;
using MarketPlaceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketPlaceApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StripeConnectController : ControllerBase
    {
        private readonly IStripeConnectService _stripeConnectService;

        public StripeConnectController(IStripeConnectService stripeConnectService)
        {
            _stripeConnectService = stripeConnectService;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        [HttpPost("onboarding-link")]
        [Authorize(Roles = "Seller,Admin")]
        public async Task<IActionResult> CreateOnboardingLink([FromBody] CreateOnboardingLinkRequestDto dto)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _stripeConnectService.CreateOrGetOnboardingLinkAsync(userId, dto);
            return Ok(result);
        }

        [HttpGet("account-status")]
        [Authorize(Roles = "Seller,Admin")]
        public async Task<IActionResult> GetAccountStatus()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var status = await _stripeConnectService.GetAccountStatusAsync(userId);
            return Ok(status);
        }

        [HttpPost("login-link")]
        [Authorize(Roles = "Seller,Admin")]
        public async Task<IActionResult> CreateLoginLink()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var link = await _stripeConnectService.CreateExpressDashboardLinkAsync(userId);
            return Ok(link);
        }

        [HttpPost("create-payment-intent")]
        public async Task<IActionResult> CreateDestinationPaymentIntent([FromBody] CreateDestinationPaymentRequestDto dto)
        {
            var result = await _stripeConnectService.CreateDestinationPaymentIntentAsync(dto);
            return Ok(result);
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var stripeSignature = Request.Headers["Stripe-Signature"];

            await _stripeConnectService.HandleWebhookAsync(json, stripeSignature!);
            return Ok();
        }
    }
}
