using Casino.Contracts.Game;
using global::Casino.Contracts.Game;
using global::Casino.Contracts.Wallet;
using global::Casino.Common;
using global::Casino.Wallet;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace Casino.Test
{
    public class WalletTests
    {
        private const string SettlementApiKey = "wallet-tests-settlement-key";
        private const int ConcurrentRequestCount = 100;

        [Test]
        public async Task ConcurrentDeposits_AllAmountsAreCredited()
        {
            using var factory = CreateWalletFactory();
            using var client = factory.CreateClient();

            var responses = await Task.WhenAll(Enumerable.Range(0, ConcurrentRequestCount)
                .Select(_ => DepositAsync(client, 1m)));

            Assert.That(responses.All(result => result.Error is null), Is.True);
            Assert.That(await GetBalanceAsync(client), Is.EqualTo(ConcurrentRequestCount));
        }

        [Test]
        public async Task ConcurrentWithdrawals_DoNotOverdrawBalance()
        {
            using var factory = CreateWalletFactory();
            using var client = factory.CreateClient();
            await DepositAsync(client, ConcurrentRequestCount);

            var results = await Task.WhenAll(Enumerable.Range(0, ConcurrentRequestCount * 2)
                .Select(_ => WithdrawAsync(client, 1m)));

            Assert.That(results.Count(result => result.Error is null), Is.EqualTo(ConcurrentRequestCount));
            Assert.That(results.Count(result => result.Error == OperationError.InsufficientFunds), Is.EqualTo(ConcurrentRequestCount));
            Assert.That(await GetBalanceAsync(client), Is.Zero);
        }

        [Test]
        public async Task ConcurrentSettlements_DoNotOverdrawBalance()
        {
            using var factory = CreateWalletFactory();
            using var client = factory.CreateClient();
            await DepositAsync(client, ConcurrentRequestCount);
            client.DefaultRequestHeaders.Add("X-Settlement-Key", SettlementApiKey);

            var results = await Task.WhenAll(Enumerable.Range(0, ConcurrentRequestCount * 2)
                .Select(_ => SettleBetAsync(client, 1m, 0m)));

            Assert.That(results.Count(result => result.Error is null), Is.EqualTo(ConcurrentRequestCount));
            Assert.That(results.Count(result => result.Error == OperationError.InsufficientFunds), Is.EqualTo(ConcurrentRequestCount));
            Assert.That(await GetBalanceAsync(client), Is.Zero);
        }

        private static WebApplicationFactory<global::Casino.Wallet.Program> CreateWalletFactory()
        {
            return new WebApplicationFactory<global::Casino.Wallet.Program>().WithWebHostBuilder(builder =>
                builder.ConfigureAppConfiguration((_, configuration) =>
                    configuration.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["SettlementApiKey"] = SettlementApiKey
                    })));
        }

        private static async Task<WalletOperationResult> DepositAsync(HttpClient client, decimal amount)
        {
            using var response = await client.PostAsJsonAsync("/api/wallet/deposit", new DepositRequest { Amount = amount });
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<WalletOperationResult>())!;
        }

        private static async Task<WalletOperationResult> WithdrawAsync(HttpClient client, decimal amount)
        {
            using var response = await client.PostAsJsonAsync("/api/wallet/withdraw", new WithdrawRequest { Amount = amount });
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<WalletOperationResult>())!;
        }

        private static async Task<BetOperationResult> SettleBetAsync(HttpClient client, decimal betAmount, decimal payoutAmount)
        {
            using var response = await client.PostAsJsonAsync(
                "/api/wallet/settlebet",
                new SettleBetRequest { BetAmount = betAmount, PayoutAmount = payoutAmount });
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<BetOperationResult>())!;
        }

        private static async Task<decimal> GetBalanceAsync(HttpClient client)
        {
            return (await client.GetFromJsonAsync<decimal>("/api/wallet/balance"));
        }
    }
}
