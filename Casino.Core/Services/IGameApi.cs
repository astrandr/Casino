using Casino.Contracts.Wallet;
using Refit;

namespace Casino.Core.Services
{
    internal interface IGameApi
    {
        [Post("/api/game/bet")]
        Task<ApiResponse<decimal>> BetAsync();

    }
}
