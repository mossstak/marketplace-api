using MarketPlaceApi.Data;
using MarketPlaceApi.Dtos;
using MarketPlaceApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using System;
using System.Threading.Tasks;

namespace MarketPlaceApi.Services
{
    public class StripeConnectService : IStripeConnectService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<StripeConnectService> _logger;

        public StripeConnectService(
            ApplicationDbContext dbContext,
            IConfiguration configuration,
            ILogger<StripeConnectService> logger)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _logger = logger;

            var secretKey = _configuration["Stripe:SecretKey"];
            if (!string.IsNullOrEmpty(secretKey))
            {
                StripeConfiguration.ApiKey = secretKey;
            }
        }

        public async Task<StripeOnboardingResponseDto> CreateOrGetOnboardingLinkAsync(string userId, CreateOnboardingLinkRequestDto dto)
        {
            var roaster = await _dbContext.RoasterProfiles
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.UserId == userId);

            if (roaster == null)
            {
                throw new InvalidOperationException("Roaster profile not found for user.");
            }

            // 1. Create Connected Express Account if not already created
            if (string.IsNullOrEmpty(roaster.StripeAccountId))
            {
                var accountOptions = new AccountCreateOptions
                {
                    Type = "express",
                    Email = string.IsNullOrWhiteSpace(roaster.User?.Email) ? null : roaster.User.Email.Trim(),

                    BusinessProfile = new AccountBusinessProfileOptions
                    {
                        Name = roaster.CompanyName ?? $"{roaster.User?.FirstName} {roaster.User?.LastName}".Trim(),
                        Url = string.IsNullOrWhiteSpace(roaster.WebsiteUrl) ? null : roaster.WebsiteUrl.Trim()
                    },

                    Capabilities = new AccountCapabilitiesOptions
                    {
                        CardPayments = new AccountCapabilitiesCardPaymentsOptions { Requested = true },
                        Transfers = new AccountCapabilitiesTransfersOptions { Requested = true },
                    },
                    Metadata = new System.Collections.Generic.Dictionary<string, string>
                    {
                        { "RoasterProfileId", roaster.Id.ToString() },
                        { "UserId", userId }
                    }
                };

                var accountService = new AccountService();
                Account account = await accountService.CreateAsync(accountOptions);

                roaster.StripeAccountId = account.Id;
                await _dbContext.SaveChangesAsync();
            }

            // 2. Generate Account Onboarding Link
            var linkOptions = new AccountLinkCreateOptions
            {
                Account = roaster.StripeAccountId,
                RefreshUrl = dto.RefreshUrl,
                ReturnUrl = dto.ReturnUrl,
                Type = "account_onboarding",
            };

            var linkService = new AccountLinkService();
            AccountLink accountLink = await linkService.CreateAsync(linkOptions);

            return new StripeOnboardingResponseDto
            {
                StripeAccountId = roaster.StripeAccountId,
                OnboardingUrl = accountLink.Url
            };
        }

        public async Task<StripeAccountStatusResponseDto> GetAccountStatusAsync(string userId)
        {
            var roaster = await _dbContext.RoasterProfiles.FirstOrDefaultAsync(r => r.UserId == userId);
            if (roaster == null || string.IsNullOrEmpty(roaster.StripeAccountId))
            {
                return new StripeAccountStatusResponseDto
                {
                    StripeAccountId = string.Empty,
                    DetailsSubmitted = false,
                    ChargesEnabled = false,
                    PayoutsEnabled = false
                };
            }

            var accountService = new AccountService();
            Account account = await accountService.GetAsync(roaster.StripeAccountId);

            bool payoutsEnabled = account.PayoutsEnabled && account.ChargesEnabled;
            if (roaster.PayoutsEnabled != payoutsEnabled)
            {
                roaster.PayoutsEnabled = payoutsEnabled;
                await _dbContext.SaveChangesAsync();
            }

            return new StripeAccountStatusResponseDto
            {
                StripeAccountId = account.Id,
                DetailsSubmitted = account.DetailsSubmitted,
                ChargesEnabled = account.ChargesEnabled,
                PayoutsEnabled = payoutsEnabled
            };
        }

        public async Task<StripeLoginLinkResponseDto> CreateExpressDashboardLinkAsync(string userId)
        {
            var roaster = await _dbContext.RoasterProfiles.FirstOrDefaultAsync(r => r.UserId == userId);
            if (roaster == null || string.IsNullOrEmpty(roaster.StripeAccountId))
            {
                throw new InvalidOperationException("Stripe connected account not set up for this roaster.");
            }

            var service = new AccountLoginLinkService();
            LoginLink loginLink = await service.CreateAsync(roaster.StripeAccountId);

            return new StripeLoginLinkResponseDto
            {
                LoginUrl = loginLink.Url
            };
        }

        public async Task<DestinationPaymentIntentResponseDto> CreateDestinationPaymentIntentAsync(CreateDestinationPaymentRequestDto dto)
        {
            if (dto.AmountInMinorUnit < 30)
            {
                throw new InvalidOperationException("The minimum payment amount is £0.30 (30p).");
            }

            var roaster = await _dbContext.RoasterProfiles.FirstOrDefaultAsync(r => r.Id == dto.RoasterProfileId);
            if (roaster == null)
            {
                throw new InvalidOperationException("The selected roaster profile was not found.");
            }

            if (string.IsNullOrEmpty(roaster.StripeAccountId))
            {
                var name = !string.IsNullOrWhiteSpace(roaster.CompanyName) ? roaster.CompanyName : "This roaster";
                throw new InvalidOperationException($"{name} has not completed their Stripe Connect payout setup yet.");
            }

            // Verify with Stripe that the connected account is active and able to receive transfers
            var accountService = new AccountService();
            var stripeAccount = await accountService.GetAsync(roaster.StripeAccountId);
            bool isReady = stripeAccount.PayoutsEnabled && stripeAccount.ChargesEnabled;

            if (roaster.PayoutsEnabled != isReady)
            {
                roaster.PayoutsEnabled = isReady;
                await _dbContext.SaveChangesAsync();
            }

            if (!isReady)
            {
                var name = !string.IsNullOrWhiteSpace(roaster.CompanyName) ? roaster.CompanyName : "This roaster";
                throw new InvalidOperationException($"{name} has not completed their Stripe onboarding yet. The seller must complete onboarding in Seller > Payouts before receiving customer payments.");
            }

            // Calculate Application Fee (Platform Commission)
            long applicationFee = dto.ApplicationFeeAmountInMinorUnit ??
                (long)Math.Round(dto.AmountInMinorUnit * (dto.FeePercentage / 100.0m));

            if (applicationFee >= dto.AmountInMinorUnit)
            {
                applicationFee = Math.Max(0, dto.AmountInMinorUnit - 1);
            }

            var receiptEmail = string.IsNullOrWhiteSpace(dto.CustomerEmail) ? null : dto.CustomerEmail.Trim();

            var options = new PaymentIntentCreateOptions
            {
                Amount = dto.AmountInMinorUnit,
                Currency = dto.Currency.ToLowerInvariant(),
                ReceiptEmail = receiptEmail,
                ApplicationFeeAmount = applicationFee,
                TransferData = new PaymentIntentTransferDataOptions
                {
                    Destination = roaster.StripeAccountId
                },
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true
                }
            };

            var service = new PaymentIntentService();
            PaymentIntent intent = await service.CreateAsync(options);

            return new DestinationPaymentIntentResponseDto
            {
                ClientSecret = intent.ClientSecret,
                PaymentIntentId = intent.Id,
                Amount = intent.Amount,
                ApplicationFeeAmount = intent.ApplicationFeeAmount ?? applicationFee,
                ConnectedAccountId = roaster.StripeAccountId
            };
        }

        public async Task HandleWebhookAsync(string jsonBody, string stripeSignature)
        {
            var webhookSecret = _configuration["Stripe:WebhookSecret"];
            Event stripeEvent;

            if (!string.IsNullOrEmpty(webhookSecret))
            {
                stripeEvent = EventUtility.ConstructEvent(jsonBody, stripeSignature, webhookSecret);
            }
            else
            {
                stripeEvent = EventUtility.ParseEvent(jsonBody);
            }

            if (stripeEvent.Type == "account.updated" || stripeEvent.Type == EventTypes.AccountUpdated)
            {
                var account = stripeEvent.Data.Object as Account;
                if (account != null)
                {
                    var roaster = await _dbContext.RoasterProfiles.FirstOrDefaultAsync(r => r.StripeAccountId == account.Id);
                    if (roaster != null)
                    {
                        roaster.PayoutsEnabled = account.PayoutsEnabled && account.ChargesEnabled;
                        await _dbContext.SaveChangesAsync();
                        _logger.LogInformation("Updated payouts status for Roaster {RoasterId} (Stripe Account {AccountId}) to {Status}",
                            roaster.Id, account.Id, roaster.PayoutsEnabled);
                    }
                }
            }

            await Task.CompletedTask;
        }
    }
}
