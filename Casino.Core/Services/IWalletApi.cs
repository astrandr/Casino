using Casino.Contracts.Wallet;
using Refit;

namespace Casino.Core.Services
{
    internal interface IWalletApi
    {
        [Get("/api/Ballance")]
        Task<ApiResponse<decimal>> GetBalanceAsync();

        [Post("/api/deposit")]
        Task<ApiResponse<WalletOperationResult>> DepositAsync([Body] DepositRequest request);

        [Post("/api/withdraw")]
        Task<ApiResponse<WalletOperationResult>> WithdrawAsync([Body] WithdrawRequest request);

        [Post("/api/settle")]
        Task<ApiResponse<WalletOperationResult>> SettleBetAsync([Body] SettleBetRequest request);
    }
}
