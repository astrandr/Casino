using Casino.Contracts.Wallet;
using Refit;

namespace Casino.Core.Services
{
    internal interface IWalletApi
    {
        [Get("/api/wallets/balance")]
        Task<ApiResponse<decimal>> GetBalanceAsync();

        [Post("/api/wallets/deposit")]
        Task<ApiResponse<WalletOperationResult>> DepositAsync([Body] DepositRequest request);

        [Post("/api/wallets/withdraw")]
        Task<ApiResponse<WalletOperationResult>> WithdrawAsync([Body] WithdrawRequest request);

        [Post("/api/wallets/settle")]
        Task<ApiResponse<BetOperationResult>> SettleBetAsync([Body] SettleBetRequest request);
    }
}
