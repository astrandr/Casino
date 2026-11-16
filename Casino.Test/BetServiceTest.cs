using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Casino.Contracts.Game;
using Casino.Game.Services;
using Casino.Common;
using Refit;

namespace Casino.Test
{
    public class BetServiceTest
    {
        [TestCase(0, 0, 90)]
        [TestCase(2, 20, 110)]
        [TestCase(5, 50, 140)]
        public async Task PlaceBetAsync_WhenFundsAreAvailable_SettlesBetAndReturnsOutcome(
            decimal betOutcome,
            decimal payoutAmount,
            decimal expectedBalance)
        {
            const decimal initialBalance = 100m;
            const decimal betAmount = 10m;
            using var walletServer = new StubWalletServer(
                JsonSerializer.Serialize(initialBalance),
                JsonSerializer.Serialize(new BetOperationResult(expectedBalance, payoutAmount)));
            var gameService = new StubGameService(betOutcome);
            var service = CreateBetService(walletServer, gameService);

            var result = await service.PlaceBetAsync(betAmount);
            await walletServer.RequestsHandled;

            Assert.That(result.Balance, Is.EqualTo(expectedBalance));
            Assert.That(result.betOutcome, Is.EqualTo(betOutcome));
            Assert.That(result.Error, Is.Null);
            Assert.That(gameService.BetCallCount, Is.EqualTo(1));
            AssertBalanceRequest(walletServer.Requests[0]);
            AssertSettlementRequest(walletServer.Requests[1], betAmount, payoutAmount);
        }

        [Test]
        public async Task PlaceBetAsync_WhenBalanceIsInsufficient_ReturnsInsufficientFundsWithoutPlaying()
        {
            const decimal currentBalance = 5m;
            const decimal betAmount = 10m;
            using var walletServer = new StubWalletServer(JsonSerializer.Serialize(currentBalance));
            var gameService = new StubGameService(2m);
            var service = CreateBetService(walletServer, gameService);

            var result = await service.PlaceBetAsync(betAmount);
            await walletServer.RequestsHandled;

            Assert.That(result.Balance, Is.EqualTo(currentBalance));
            Assert.That(result.betOutcome, Is.Zero);
            Assert.That(result.Error, Is.EqualTo(OperationError.InsufficientFunds));
            Assert.That(gameService.BetCallCount, Is.Zero);
            Assert.That(walletServer.Requests, Has.Count.EqualTo(1));
            AssertBalanceRequest(walletServer.Requests[0]);
        }

        [Test]
        public async Task PlaceBetAsync_WhenBalanceRequestFails_PropagatesHttpErrorWithoutPlaying()
        {
            using var walletServer = new StubWalletServer(HttpStatusCode.InternalServerError, "wallet unavailable");
            var gameService = new StubGameService(2m);
            var service = CreateBetService(walletServer, gameService);

            var exception = Assert.ThrowsAsync<ApiException>(async () => await service.PlaceBetAsync(10m));
            await walletServer.RequestsHandled;

            Assert.That(exception!.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
            Assert.That(walletServer.Requests, Has.Count.EqualTo(1));
            AssertBalanceRequest(walletServer.Requests[0]);
            Assert.That(gameService.BetCallCount, Is.Zero);
        }

        [Test]
        public async Task PlaceBetAsync_WhenWalletSettlementFails_ReturnsErrorAndZeroOutcome()
        {
            const decimal initialBalance = 100m;
            const decimal betAmount = 10m;
            using var walletServer = new StubWalletServer(
                JsonSerializer.Serialize(initialBalance),
                JsonSerializer.Serialize(new BetOperationResult(initialBalance, 0m, OperationError.InsufficientFunds)));
            var gameService = new StubGameService(2m);
            var service = CreateBetService(walletServer, gameService);

            var result = await service.PlaceBetAsync(betAmount);
            await walletServer.RequestsHandled;

            Assert.That(result.Balance, Is.EqualTo(initialBalance));
            Assert.That(result.betOutcome, Is.Zero);
            Assert.That(result.Error, Is.EqualTo(OperationError.InsufficientFunds));
            Assert.That(gameService.BetCallCount, Is.EqualTo(1));
            AssertSettlementRequest(walletServer.Requests[1], betAmount, 20m);
        }

        private static BetService CreateBetService(StubWalletServer walletServer, IGameService gameService)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["WalletBaseUrl"] = walletServer.BaseUrl,
                    ["SettlementApiKey"] = "test-settlement-key"
                })
                .Build();

            return new BetService(gameService, configuration);
        }

        private static void AssertBalanceRequest(CapturedRequest request)
        {
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.Path, Is.EqualTo("/api/wallet/balance"));
        }

        private static void AssertSettlementRequest(CapturedRequest request, decimal betAmount, decimal payoutAmount)
        {
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.Path, Is.EqualTo("/api/wallet/settlebet"));

            using var body = JsonDocument.Parse(request.Body);
            Assert.That(GetDecimalProperty(body.RootElement, "BetAmount"), Is.EqualTo(betAmount));
            Assert.That(GetDecimalProperty(body.RootElement, "PayoutAmount"), Is.EqualTo(payoutAmount));
        }

        private static decimal GetDecimalProperty(JsonElement element, string propertyName)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    return property.Value.GetDecimal();
                }
            }

            Assert.Fail($"Request JSON is missing property '{propertyName}'.");
            return default;
        }

        private sealed class StubGameService : IGameService
        {
            private readonly decimal betOutcome;

            public StubGameService(decimal betOutcome)
            {
                this.betOutcome = betOutcome;
            }

            public int BetCallCount { get; private set; }

            public decimal Bet()
            {
                BetCallCount++;
                return betOutcome;
            }
        }

        private sealed class StubWalletServer : IDisposable
        {
            private readonly HttpListener listener = new();
            private readonly string[] responseBodies;
            private readonly HttpStatusCode statusCode;

            public StubWalletServer(params string[] responseBodies)
                : this(HttpStatusCode.OK, responseBodies)
            {
            }

            public StubWalletServer(HttpStatusCode statusCode, params string[] responseBodies)
            {
                this.responseBodies = responseBodies;
                this.statusCode = statusCode;

                using var portListener = new TcpListener(IPAddress.Loopback, 0);
                portListener.Start();
                var port = ((IPEndPoint)portListener.LocalEndpoint).Port;
                portListener.Stop();

                BaseUrl = $"http://127.0.0.1:{port}/";
                listener.Prefixes.Add(BaseUrl);
                listener.Start();
                RequestsHandled = HandleRequestsAsync();
            }

            public string BaseUrl { get; }
            public List<CapturedRequest> Requests { get; } = new();
            public Task RequestsHandled { get; }

            private async Task HandleRequestsAsync()
            {
                foreach (var responseBody in responseBodies)
                {
                    var context = await listener.GetContextAsync();
                    string requestBody;
                    using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                    {
                        requestBody = await reader.ReadToEndAsync();
                    }

                    Requests.Add(new CapturedRequest(
                        context.Request.HttpMethod,
                        context.Request.Url?.AbsolutePath ?? string.Empty,
                        requestBody));

                    var responseBytes = Encoding.UTF8.GetBytes(responseBody);
                    context.Response.StatusCode = (int)statusCode;
                    context.Response.ContentType = "application/json";
                    context.Response.ContentLength64 = responseBytes.Length;
                    await context.Response.OutputStream.WriteAsync(responseBytes);
                    context.Response.Close();
                }
            }

            public void Dispose()
            {
                listener.Close();
            }
        }

        private sealed record CapturedRequest(string Method, string Path, string Body);
    }
}
