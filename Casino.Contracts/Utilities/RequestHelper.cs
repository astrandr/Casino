using Refit;
using System.Net;

namespace Casino.Common.Utilities
{
    public class RequestHelper
    {
        public static async Task<T> SendRequestAsync<T>(Func<Task<ApiResponse<T>>> send)
        {
            var response = await send();

            bool isRetryable = response.StatusCode is
                                    HttpStatusCode.BadGateway or
                                    HttpStatusCode.ServiceUnavailable or
                                    HttpStatusCode.GatewayTimeout;

            //await response.EnsureSuccessfulAsync();

            return response.Content ?? throw new InvalidOperationException($"Target service is not responding. Error: {response.StatusCode}");
        }
    }
}
