using Casino.Contracts.Wallet;

namespace Casino.Wallet.Repository
{
    public interface IWalletRepository
    {
        Task<decimal> GetBalanceAsync();

        Task<WalletOperationResult> DepositAsync(string requestId, decimal amount);

        Task<WalletOperationResult> WithdrawAsync(string requestId, decimal amount);

        Task<BetOperationResult> SettleBetAsync(string requestId, decimal betAmount, decimal payout);
    }
}
