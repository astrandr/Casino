using Casino.Contracts.Game;
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

        public async Task<WalletOperationResult> DepositAsync(decimal amount)
        {          
            return await walletRepository.DepositAsync(amount);
        }

        public async Task<decimal> GetBalanceAsync()
        {
            return await walletRepository.GetBalanceAsync();
        }

        public async Task<BetOperationResult> SettleBetAsync(decimal betAmount, decimal payout)
        {
            return await walletRepository.SettleBetAsync(betAmount, payout);
        }

        public async Task<WalletOperationResult> WithdrawAsync(decimal amount)
        {
            return await walletRepository.WithdrawAsync(amount);
        }
    }
}
