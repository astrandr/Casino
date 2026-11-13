using Casino.Contracts.Wallet;

namespace Casino.Wallet.Repository
{
    public class WalletRepository : IWalletRepository
    {
        public Task<WalletOperationResult> DepositAsync(string requestId, decimal amount)
        {
            throw new NotImplementedException();
        }

        public Task<decimal> GetBalanceAsync()
        {
            throw new NotImplementedException();
        }

        public Task<BetOperationResult> SettleBetAsync(string requestId, decimal betAmount, decimal payout)
        {
            throw new NotImplementedException();
        }

        public Task<WalletOperationResult> WithdrawAsync(string requestId, decimal amount)
        {
            throw new NotImplementedException();
        }
    }
}
