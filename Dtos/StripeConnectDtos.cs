namespace MarketPlaceApi.Dtos
{
    public class CreateOnboardingLinkRequestDto
    {
        public string RefreshUrl { get; set; } = "http://localhost:5173/roaster/payouts/refresh";
        public string ReturnUrl { get; set; } = "http://localhost:5173/roaster/payouts/return";
    }

    public class StripeOnboardingResponseDto
    {
        public string StripeAccountId { get; set; } = string.Empty;
        public string OnboardingUrl { get; set; } = string.Empty;
    }

    public class StripeAccountStatusResponseDto
    {
        public string StripeAccountId { get; set; } = string.Empty;
        public bool DetailsSubmitted { get; set; }
        public bool ChargesEnabled { get; set; }
        public bool PayoutsEnabled { get; set; }
    }

    public class StripeLoginLinkResponseDto
    {
        public string LoginUrl { get; set; } = string.Empty;
    }

    public class CreateDestinationPaymentRequestDto
    {
        public long AmountInMinorUnit { get; set; } // e.g. 3000 = £30.00
        public string Currency { get; set; } = "gbp";
        public string? CustomerEmail { get; set; }
        public int RoasterProfileId { get; set; }
        public long? ApplicationFeeAmountInMinorUnit { get; set; } // Optional fixed app fee
        public decimal FeePercentage { get; set; } = 5.0m; // Default 5% platform commission
    }

    public class DestinationPaymentIntentResponseDto
    {
        public string ClientSecret { get; set; } = string.Empty;
        public string PaymentIntentId { get; set; } = string.Empty;
        public long Amount { get; set; }
        public long ApplicationFeeAmount { get; set; }
        public string ConnectedAccountId { get; set; } = string.Empty;
    }
}
