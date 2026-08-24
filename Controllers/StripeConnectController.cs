using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using MarketPlaceApi.Dtos;
using MarketPlaceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

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

            try
            {
                var result = await _stripeConnectService.CreateOrGetOnboardingLinkAsync(userId, dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (StripeException ex)
            {
                return BadRequest(new { message = ex.StripeError?.Message ?? ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while generating the onboarding link." });
            }
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

            try
            {
                var status = await _stripeConnectService.GetAccountStatusAsync(userId);
                return Ok(status);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (StripeException ex)
            {
                return BadRequest(new { message = ex.StripeError?.Message ?? ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving Stripe account status." });
            }
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

            try
            {
                var link = await _stripeConnectService.CreateExpressDashboardLinkAsync(userId);
                return Ok(link);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (StripeException ex)
            {
                return BadRequest(new { message = ex.StripeError?.Message ?? ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while generating the dashboard login link." });
            }
        }

        [HttpPost("create-payment-intent")]
        public async Task<IActionResult> CreateDestinationPaymentIntent([FromBody] CreateDestinationPaymentRequestDto dto)
        {
            try
            {
                var result = await _stripeConnectService.CreateDestinationPaymentIntentAsync(dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (StripeException ex)
            {
                return BadRequest(new { message = ex.StripeError?.Message ?? ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An unexpected error occurred while creating the payment intent." });
            }
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            try
            {
                var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                var stripeSignature = Request.Headers["Stripe-Signature"];

                await _stripeConnectService.HandleWebhookAsync(json, stripeSignature!);
                return Ok();
            }
            catch (StripeException ex)
            {
                return BadRequest(new { message = ex.StripeError?.Message ?? ex.Message });
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
    }
}
