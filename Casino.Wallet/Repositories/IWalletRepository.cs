using Casino.Contracts.Game;
using Casino.Contracts.Wallet;

namespace Casino.Wallet.Repository
{
    public interface IWalletRepository
    {
        Task<decimal> GetBalanceAsync();

        Task<WalletOperationResult> DepositAsync(decimal amount);

        Task<WalletOperationResult> WithdrawAsync(decimal amount);

        Task<BetOperationResult> SettleBetAsync(decimal betAmount, decimal payout);
    }
}
