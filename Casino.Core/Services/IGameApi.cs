using Casino.Contracts.Wallet;
using Refit;

namespace Casino.Core.Services
{
    internal interface IGameApi
    {
        [Post("/api/deposit")]
        Task<ApiResponse<decimal>> BetAsync();

    }
}
