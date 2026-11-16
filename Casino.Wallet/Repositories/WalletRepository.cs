using Casino.Common;
using Casino.Contracts.Game;
using Casino.Contracts.Wallet;

namespace Casino.Wallet.Repository
{
    public class WalletRepository : IWalletRepository
    {
        private readonly object syncObject = new ();

        private decimal balance = 0;

        public Task<WalletOperationResult> DepositAsync(decimal amount)
        {
            decimal updatedBalance = 0;

            lock (syncObject)
            {
                if (amount <= 0)
                {
                    return Task.FromResult(new WalletOperationResult(balance, OperationError.InvalidAmount));
                }
                balance += amount;
                updatedBalance = balance;
            }

            return Task.FromResult(new WalletOperationResult(updatedBalance));
        }

        public Task<decimal> GetBalanceAsync()
        {
            decimal currentBalance = 0;
            
            lock (syncObject)
            {
                currentBalance = balance;
            }
            
            return Task.FromResult(currentBalance);
        }

        public Task<BetOperationResult> SettleBetAsync(decimal betAmount, decimal payout)
        {

            decimal updatedBalance = 0;

            lock (syncObject)
            {
                if (betAmount < 1 || betAmount > 10 || payout < 0)
                {
                    return Task.FromResult(new BetOperationResult(balance, betAmount, OperationError.InvalidAmount));
                }

                if (betAmount > balance)
                {
                    return Task.FromResult(new BetOperationResult(balance, payout, OperationError.InsufficientFunds));
                }

                balance = balance - betAmount + payout;
                updatedBalance = balance;
            }

            return Task.FromResult(new BetOperationResult(updatedBalance, payout));
        }

        public Task<WalletOperationResult> WithdrawAsync(decimal amount)
        {
            decimal updatedBalance = 0;

            lock (syncObject)
            {
                if (amount <= 0)
                {
                    return Task.FromResult(new WalletOperationResult(balance, OperationError.InvalidAmount));
                }

                if (amount > balance)
                {
                    return Task.FromResult(new WalletOperationResult(balance, OperationError.InsufficientFunds));
                }

                balance -= amount;
                updatedBalance = balance;
            }

            return Task.FromResult(new WalletOperationResult(updatedBalance));
        }
    }
}
