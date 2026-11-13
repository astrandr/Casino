using global::Casino.Core.Services;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Casino.Test
{
    public class CasinoServiceTests
    {
        [Test]
        public async Task DepositAsync_SendsAmountAndReturnsWalletResult()
        {
            using var server = new StubWalletServer("{\"balance\":125.50,\"requestId\":\"00000000-0000-0000-0000-000000000001\",\"error\":null}");

            var service = CreateWalletService(server);

            var result = await service.DepositAsync(25.50m);
            await server.RequestHandled;

            Assert.That(result.Balance, Is.EqualTo(125.50m));
            Assert.That(result.requestId, Is.EqualTo(Guid.Parse("00000000-0000-0000-0000-000000000001")));
            AssertRequest(server, "api/deposit", "amount", 25.50m);
        }

        [Test]
        public async Task WithdrawAsync_SendsAmountAndReturnsWalletResult()
        {
            using var server = new StubWalletServer("{\"balance\":74.50,\"requestId\":\"00000000-0000-0000-0000-000000000001\",\"error\":null}");
            var service = CreateWalletService(server);

            var result = await service.WithdrawAsync(25.50m);
            await server.RequestHandled;

            Assert.That(result.Balance, Is.EqualTo(74.50m));
            Assert.That(result.requestId, Is.EqualTo(Guid.Parse("00000000-0000-0000-0000-000000000001")));
            AssertRequest(server, "api/withdraw", "amount", 25.50m);
        }

        [TestCase(10, 0, TestName = "BetAsync_LosesBet_ReturnsZero")]
        [TestCase(10, 20, TestName = "BetAsync_WinsTwoTimesBet_ReturnsTwoTimesBet")]
        [TestCase(10, 50, TestName = "BetAsync_WinsBetweenTwoAndTenTimesBet_ReturnsPayout")]
        [TestCase(10, 100, TestName = "BetAsync_WinsTenTimesBet_ReturnsTenTimesBet")]
        public async Task BetAsync_SendsAmountAndReturnsBetOutcome(decimal amount, decimal expectedBetOutcome)
        {
            const decimal oldBalance = 100m;
            var expectedBalance = oldBalance - amount + expectedBetOutcome;
            var responseBody = JsonSerializer.Serialize(new
            {
                balance = expectedBalance,
                betOutcome = expectedBetOutcome,
                requestId = "00000000-0000-0000-0000-000000000001",
                error = (string?)null
            });
            using var server = new StubGameServer(responseBody);
            var service = CreateGameService(server);

            var result = await service.BetAsync(amount);
            await server.RequestHandled;

            Assert.That(result.Balance, Is.EqualTo(expectedBalance));
            Assert.That(result.betOutcome, Is.EqualTo(expectedBetOutcome));
            AssertRequest(server, "api/deposit", "amount", amount);
        }

        [Test]
        public async Task BetAsync_WhenGameServiceBetOperationFails_ThrowsApiException()
        {
            using var server = new StubGameServer("game service unavailable", HttpStatusCode.InternalServerError);

            var service = CreateGameService(server);

            Assert.ThrowsAsync<Refit.ApiException>(async () => await service.BetAsync(10m));

            await server.RequestHandled;

            AssertRequest(server, "api/deposit", "amount", 10m);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task OperationAsync_GeneratesRequestId(bool deposit)
        {
            using var server = new StubWalletServer("{\"balance\":10,\"requestId\":\"00000000-0000-0000-0000-000000000001\",\"error\":null}");
            var service = CreateWalletService(server);

            if (deposit)
            {
                await service.DepositAsync(1m);
            }
            else
            {
                await service.WithdrawAsync(1m);
            }

            await server.RequestHandled;
            using var requestJson = JsonDocument.Parse(server.RequestBody!);
            Assert.That(Guid.TryParse(GetPropertyIgnoringCase(requestJson.RootElement, "requestId").GetString(), out _), Is.True);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task OperationAsync_WhenWalletReturnsFailure_ThrowsApiException(bool deposit)
        {
            using var server = new StubWalletServer("wallet unavailable", HttpStatusCode.InternalServerError);
            var service = CreateWalletService(server);

            Assert.ThrowsAsync<Refit.ApiException>(async () =>
            {
                if (deposit)
                {
                    await service.DepositAsync(1m);
                }
                else
                {
                    await service.WithdrawAsync(1m);
                }
            });
            await server.RequestHandled;
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task OperationAsync_WhenWalletReturnsNoResult_ThrowsInvalidOperationException(bool deposit)
        {
            using var server = new StubWalletServer(string.Empty, HttpStatusCode.NoContent);
            var service = CreateWalletService(server);

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                if (deposit)
                {
                    await service.DepositAsync(1m);
                }
                else
                {
                    await service.WithdrawAsync(1m);
                }
            });
            await server.RequestHandled;
        }

        private static CasinoService CreateWalletService(StubWalletServer server)
        {
            return new CasinoService(server.BaseUrl, "http://game.invalid/");
        }

        private static CasinoService CreateGameService(StubGameServer server)
        {
            return new CasinoService("http://wallet.invalid/", server.BaseUrl);
        }

        private static void AssertRequest(StubApiServer server, string expectedPath, string expectedProperty, decimal expectedAmount)
        {
            Assert.That(server.RequestUri?.AbsolutePath, Is.EqualTo("/" + expectedPath));
            Assert.That(server.RequestMethod, Is.EqualTo("POST"));

            using var requestJson = JsonDocument.Parse(server.RequestBody!);
            Assert.That(GetPropertyIgnoringCase(requestJson.RootElement, expectedProperty).GetDecimal(), Is.EqualTo(expectedAmount));
            Assert.That(Guid.TryParse(GetPropertyIgnoringCase(requestJson.RootElement, "requestId").GetString(), out _), Is.True);
        }

        private static JsonElement GetPropertyIgnoringCase(JsonElement element, string propertyName)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    return property.Value;
                }
            }

            Assert.Fail($"Request JSON is missing property '{propertyName}'.");
            return default;
        }

        private abstract class StubApiServer : IDisposable
        {
            private readonly HttpListener listener = new();
            private readonly string responseBody;
            private readonly HttpStatusCode statusCode;

            protected StubApiServer(string responseBody, HttpStatusCode statusCode = HttpStatusCode.OK)
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

        private sealed class StubWalletServer : StubApiServer
        {
            public StubWalletServer(string responseBody, HttpStatusCode statusCode = HttpStatusCode.OK)
                : base(responseBody, statusCode)
            {
            }
        }

        private sealed class StubGameServer : StubApiServer
        {
            public StubGameServer(string responseBody, HttpStatusCode statusCode = HttpStatusCode.OK)
                : base(responseBody, statusCode)
            {
            }
        }
    }
}
