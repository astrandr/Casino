using Casino.Contracts.Game;
using Casino.Contracts.Wallet;
using Refit;

namespace Casino.Core.Services
{
    internal interface IWalletApi
    {
        [Get("/api/wallet/balance")]
        Task<ApiResponse<decimal>> GetBalanceAsync();

        [Post("/api/wallet/deposit")]
        Task<ApiResponse<WalletOperationResult>> DepositAsync([Body] DepositRequest request);

        [Post("/api/wallet/withdraw")]
        Task<ApiResponse<WalletOperationResult>> WithdrawAsync([Body] WithdrawRequest request);

        [Post("/api/wallet/settlebet")]
        Task<ApiResponse<BetOperationResult>> SettleBetAsync([Body] SettleBetRequest request);
    }
}
