using MarketPlaceApi.Dtos;

namespace MarketPlaceApi.Services
{
    public interface IStripeConnectService
    {
        Task<StripeOnboardingResponseDto> CreateOrGetOnboardingLinkAsync(string userId, CreateOnboardingLinkRequestDto dto);
        Task<StripeAccountStatusResponseDto> GetAccountStatusAsync(string userId);
        Task<StripeLoginLinkResponseDto> CreateExpressDashboardLinkAsync(string userId);
        Task<DestinationPaymentIntentResponseDto> CreateDestinationPaymentIntentAsync(CreateDestinationPaymentRequestDto dto);
        Task HandleWebhookAsync(string jsonBody, string stripeSignature);
    }
}
