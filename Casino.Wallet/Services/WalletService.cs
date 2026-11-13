
using Casino.Contracts.Wallet;
using Casino.Wallet.Repository;

namespace Casino.Wallet.Services
{
    public class WalletService : IWalletService
    {
        private readonly IWalletRepository walletRepository;
 
        public WalletService(IWalletRepository walletService)
        {
            this.walletRepository = walletService;
        }

        public async Task<WalletOperationResult> DepositAsync(string requestId, decimal amount)
        {          
            return await walletRepository.DepositAsync(requestId, amount);
        }

        public async Task<decimal> GetBalanceAsync()
        {
            return await walletRepository.GetBalanceAsync();
        }

        public async Task<BetOperationResult> SettleBetAsync(string requestId, decimal betAmount, decimal payout)
        {
            return await walletRepository.SettleBetAsync(requestId, betAmount, payout);
        }

        public async Task<WalletOperationResult> WithdrawAsync(string requestId, decimal amount)
        {
            return await walletRepository.WithdrawAsync(requestId, amount);
        }
    }
}
