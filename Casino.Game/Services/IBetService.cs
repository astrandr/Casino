using Casino.Contracts.Game;

namespace Casino.Game.Services
{
    public interface IBetService
    {
        Task<BetOperationResult> PlaceBetAsync(decimal betAmount);
    }
}
