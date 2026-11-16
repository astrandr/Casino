using Casino.Contracts.Game;
using Refit;

namespace Casino.Core.Services
{
    internal interface IGameApi
    {
        [Post("/api/game/bet")]
        Task<ApiResponse<BetOperationResult>> BetAsync([Body] BetRequest request);

    }
}
