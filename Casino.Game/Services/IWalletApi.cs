using Casino.Contracts.Game;
using Casino.Contracts.Wallet;
using Refit;

namespace Casino.Game.Services
{
    internal interface IWalletApi
    {
        [Get("/api/wallet/balance")]
        Task<ApiResponse<decimal>> GetBalanceAsync();

        [Post("/api/wallet/settlebet")]
        Task<ApiResponse<BetOperationResult>> SettleBetAsync([Body] SettleBetRequest request, [Header("X-Settlement-Key")] string apiKey);
    }
}
