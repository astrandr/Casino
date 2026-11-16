using Casino.Contracts.Game;
using Casino.Contracts.Wallet;

namespace Casino.Wallet.Services
{
    public interface IWalletService
    {
        Task<decimal> GetBalanceAsync();

        Task<WalletOperationResult> DepositAsync(decimal amount);

        Task<WalletOperationResult> WithdrawAsync(decimal amount);

        Task<BetOperationResult> SettleBetAsync(decimal betAmount, decimal payout);
    }
}
