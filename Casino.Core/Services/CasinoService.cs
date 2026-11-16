using Casino.Common.Utilities;
using Casino.Contracts.Game;
using Casino.Contracts.Wallet;
using Refit;

namespace Casino.Core.Services
{
    public class CasinoService(string walletBaseUrl, string gameBaseUrl) : ICasinoService
    {
        private readonly IWalletApi walletApi = RestService.For<IWalletApi>(walletBaseUrl);
        private readonly IGameApi gameApi = RestService.For<IGameApi>(gameBaseUrl);

        public async Task<BetOperationResult> BetAsync(decimal betAmount)
        {
            var request = new BetRequest { BetAmount = betAmount };            

            try
            {
                return await RequestHelper.SendRequestAsync(() => gameApi.BetAsync(request));
            }
            catch (InvalidOperationException ex)
            {
                throw new Exception("Bet:Game:" + ex.Message);
            }
        }

        public async Task<WalletOperationResult> WithdrawAsync(decimal amount)
        {
            var request = new WithdrawRequest { Amount = amount };            

            try
            {
                return await RequestHelper.SendRequestAsync(() => walletApi.WithdrawAsync(request));
            }
            catch (InvalidOperationException ex)
            {
                throw new Exception("Withdraw:Wallet:" + ex.Message);
            }
        }


        public async Task<WalletOperationResult> DepositAsync(decimal amount)
        {
            var request = new DepositRequest { Amount = amount };

            try
            {
                return await RequestHelper.SendRequestAsync(() => walletApi.DepositAsync(request));
            }
            catch (InvalidOperationException ex)
            {
                throw new Exception("Deposit:Wallet:"+ex.Message);
            }            
        }        
    }
}
