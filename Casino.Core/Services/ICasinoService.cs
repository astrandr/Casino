using Casino.Contracts.Wallet;

namespace Casino.Core.Services
{
    public interface ICasinoService
    {
        Task<WalletOperationResult> DepositAsync(decimal amount);

        Task<WalletOperationResult> WithdrawAsync(decimal amount);

        Task<BetOperationResult> BetAsync(decimal amount);
    }
}
