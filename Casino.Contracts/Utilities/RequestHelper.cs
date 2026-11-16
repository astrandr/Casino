using Refit;
using System.Net;

namespace Casino.Common.Utilities
{
    public class RequestHelper
    {
        public static async Task<T> SendRequestAsync<T>(Func<Task<ApiResponse<T>>> send)
        {
            using var response = await send();

            await response.EnsureSuccessfulAsync();

            return response.Content ?? throw new InvalidOperationException($"The target service returned no result. Error: {response.StatusCode}");
        }
    }
}
