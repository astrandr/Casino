using Casino.Common;
using Casino.Contracts.Wallet;
using global::Casino.Core.Services;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Refit;

namespace Casino.Test
{
    public class CasinoServiceTests
    {
        [TestCase("deposit", "/api/wallet/deposit")]
        [TestCase("withdraw", "/api/wallet/withdraw")]
        public async Task WalletOperationAsync_SendsAmountAndReturnsResult(string operation, string expectedPath)
        {
            const decimal amount = 25.50m;
            const decimal expectedBalance = 125.75m;
            using var server = new StubWalletServer(JsonSerializer.Serialize(new
            {
                balance = expectedBalance,
                error = (string?)null
            }));
            var service = CreateService(server);

            var result = await CallOperationAsync(service, operation, amount);
            await server.RequestHandled;

            Assert.That(result.Balance, Is.EqualTo(expectedBalance));
            AssertRequest(server, expectedPath, amount);
        }

        [Test]
        public async Task WithdrawAsync_WhenAmountExceedsBalance_ReturnsInsufficientFunds()
        {
            const decimal currentBalance = 20m;
            const decimal withdrawalAmount = 25m;
            var responseBody = JsonSerializer.Serialize(
                new WalletOperationResult(currentBalance, OperationError.InsufficientFunds));
            using var server = new StubWalletServer(responseBody);
            var service = CreateService(server);

            var result = await service.WithdrawAsync(withdrawalAmount);
            await server.RequestHandled;

            Assert.That(result.Balance, Is.EqualTo(currentBalance));
            Assert.That(result.Error, Is.EqualTo(OperationError.InsufficientFunds));
            AssertRequest(server, "/api/wallet/withdraw", withdrawalAmount);
        }

        [TestCase("deposit", "/api/wallet/deposit")]
        [TestCase("withdraw", "/api/wallet/withdraw")]
        public async Task WalletOperationAsync_WhenWalletReturnsFailure_ThrowsApiException(
            string operation,
            string expectedPath)
        {
            using var server = new StubWalletServer("wallet unavailable", HttpStatusCode.InternalServerError);
            var service = CreateService(server);

            var exception = Assert.ThrowsAsync<ApiException>(async () => await CallOperationAsync(service, operation, 1m));
            await server.RequestHandled;

            Assert.That(exception!.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
            Assert.That(server.RequestUri?.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(server.RequestMethod, Is.EqualTo("POST"));
        }

        [TestCase("deposit", "Deposit:Wallet:")]
        [TestCase("withdraw", "Withdraw:Wallet:")]
        public async Task WalletOperationAsync_WhenWalletReturnsNoResult_ThrowsWrappedException(
            string operation,
            string expectedMessagePrefix)
        {
            using var server = new StubWalletServer(string.Empty, HttpStatusCode.NoContent);
            var service = CreateService(server);

            var exception = Assert.ThrowsAsync<Exception>(async () => await CallOperationAsync(service, operation, 1m));
            await server.RequestHandled;

            Assert.That(exception!.Message, Does.StartWith(expectedMessagePrefix));
        }

        private static CasinoService CreateService(StubWalletServer server)
        {
            return new CasinoService(server.BaseUrl, "http://game.invalid/");
        }

        private static Task<WalletOperationResult> CallOperationAsync(CasinoService service, string operation, decimal amount)
        {
            return operation switch
            {
                "deposit" => service.DepositAsync(amount),
                "withdraw" => service.WithdrawAsync(amount),
                _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, "Unsupported wallet operation.")
            };
        }

        private static void AssertRequest(StubWalletServer server, string expectedPath, decimal expectedAmount)
        {
            Assert.That(server.RequestUri?.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(server.RequestMethod, Is.EqualTo("POST"));

            using var requestJson = JsonDocument.Parse(server.RequestBody!);
            var amountProperty = requestJson.RootElement.EnumerateObject()
                .First(property => string.Equals(property.Name, "amount", StringComparison.OrdinalIgnoreCase));
            Assert.That(amountProperty.Value.GetDecimal(), Is.EqualTo(expectedAmount));
        }

        private sealed class StubWalletServer : IDisposable
        {
            private readonly HttpListener listener = new();
            private readonly string responseBody;
            private readonly HttpStatusCode statusCode;

            public StubWalletServer(string responseBody, HttpStatusCode statusCode = HttpStatusCode.OK)
            {
                this.responseBody = responseBody;
                this.statusCode = statusCode;

                using var portListener = new TcpListener(IPAddress.Loopback, 0);
                portListener.Start();
                var port = ((IPEndPoint)portListener.LocalEndpoint).Port;
                portListener.Stop();

                BaseUrl = $"http://127.0.0.1:{port}/";
                listener.Prefixes.Add(BaseUrl);
                listener.Start();
                RequestHandled = HandleRequestAsync();
            }

            public string BaseUrl { get; }
            public Task RequestHandled { get; }
            public Uri? RequestUri { get; private set; }
            public string? RequestMethod { get; private set; }
            public string? RequestBody { get; private set; }

            private async Task HandleRequestAsync()
            {
                var context = await listener.GetContextAsync();
                RequestUri = context.Request.Url;
                RequestMethod = context.Request.HttpMethod;

                using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                {
                    RequestBody = await reader.ReadToEndAsync();
                }

                var responseBytes = Encoding.UTF8.GetBytes(responseBody);
                context.Response.StatusCode = (int)statusCode;
                context.Response.ContentType = "application/json";
                context.Response.ContentLength64 = responseBytes.Length;
                await context.Response.OutputStream.WriteAsync(responseBytes);
                context.Response.Close();
            }

            public void Dispose()
            {
                listener.Close();
            }
        }
    }
}
