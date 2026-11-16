using Casino.Common;
using Casino.Common.Utilities;
using Casino.Contracts.Game;
using Casino.Contracts.Wallet;
using Refit;

namespace Casino.Game.Services
{
    public class BetService : IBetService
    {
        private readonly IWalletApi walletApi;
        private readonly IGameService gameService;
        private readonly string settlementApiKey;

        public BetService(IGameService gameService, IConfiguration configuration)
        {
            var walletBaseUrl = configuration["WalletBaseUrl"] ?? throw new InvalidOperationException("WalletBaseUrl is missing.");
            walletApi = RestService.For<IWalletApi>(walletBaseUrl);

            settlementApiKey = configuration["SettlementApiKey"] ?? throw new InvalidOperationException("SettlementApiKey is missing.");

            this.gameService = gameService;
        }

        public async Task<BetOperationResult> PlaceBetAsync(decimal betAmount)
        {
            if (betAmount < 1 || betAmount > 10)
            {
                return new BetOperationResult(0, 0, OperationError.InvalidAmount);
            }

            var balance = await RequestHelper.SendRequestAsync<decimal>(() => walletApi.GetBalanceAsync());

            if (balance < betAmount)
            {
                return new BetOperationResult(balance, 0, OperationError.InsufficientFunds);
            }

            var betOutcome = gameService.Bet();

            decimal payoutAmount = betAmount * betOutcome;

            var request = new SettleBetRequest { BetAmount = betAmount, PayoutAmount = payoutAmount };

            var settlement = await RequestHelper.SendRequestAsync(() => walletApi.SettleBetAsync(request, settlementApiKey));

            return new BetOperationResult(settlement.Balance, settlement.Error is null ? betOutcome : 0m, settlement.Error);

        }
    }
}
