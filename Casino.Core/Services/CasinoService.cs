using Casino.Contracts.Wallet;
using Refit;
using System.Net;

namespace Casino.Core.Services
{
    public class CasinoService : ICasinoService
    {
        private readonly IWalletApi walletApi;
        private readonly IGameApi gameApi;
        private const int MaxAttemps = 5;
        private const int RETRY_WAITTIME = 200;

        public CasinoService(string walletBaseUrl, string gameBaseUrl)
        {
            walletApi = RestService.For<IWalletApi>(walletBaseUrl);
            gameApi = RestService.For<IGameApi>(gameBaseUrl);
        }

        public async Task<BetOperationResult> BetAsync(decimal betAmount)
        {

            var balance = await SendWithRetryAsync<decimal>(() => walletApi.GetBalanceAsync());

            if (balance < betAmount)
            {
                return new BetOperationResult(balance, 0, WalletError.InsufficientFunds);
            }

            var betOutcome = await SendWithRetryAsync<decimal>(() => gameApi.BetAsync());
            
            decimal payoutAmount = betAmount * betOutcome;

            decimal newBalance = balance - betAmount + payoutAmount;


            var request = new SettleBetRequest { RequestId = Guid.NewGuid().ToString(), BetAmount = betAmount, PayoutAmount = payoutAmount };

            var settlement = await SendWithRetryAsync(() => walletApi.SettleBetAsync(request));

            return new BetOperationResult(settlement.Balance, settlement.Error is null ? betOutcome : 0m, settlement.Error);

        }

        public async Task<WalletOperationResult> WithdrawAsync(decimal amount)
        {
            var request = new WithdrawRequest { RequestId = Guid.NewGuid().ToString(), Amount = amount };

            return await SendWithRetryAsync(() => walletApi.WithdrawAsync(request));
        }


        public async Task<WalletOperationResult> DepositAsync(decimal amount)
        {
            var request = new DepositRequest { RequestId = Guid.NewGuid().ToString(), Amount = amount };

            return await SendWithRetryAsync(() => walletApi.DepositAsync(request));
        }

        private static async Task<T> SendWithRetryAsync<T>(Func<Task<ApiResponse<T>>> send) 
        {
            for (int attempt = 1; ; attempt++)
            {
                using (var response = await send())
                {
                    bool isRetryable = response.StatusCode is
                                            HttpStatusCode.BadGateway or
                                            HttpStatusCode.ServiceUnavailable or
                                            HttpStatusCode.GatewayTimeout;

                    if (attempt == MaxAttemps || !isRetryable)
                    {
                        await response.EnsureSuccessfulAsync();

                        return response.Content ?? throw new InvalidOperationException("Wallet returned no result.");
                    }
                }

                await Task.Delay(RETRY_WAITTIME);
            }
        }
    }
}
