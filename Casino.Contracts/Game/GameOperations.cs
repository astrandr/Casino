using Casino.Common;

namespace Casino.Contracts.Game
{
    public struct BetRequest
    {
        public decimal BetAmount { get; set; }
    }

    public sealed record BetOperationResult(decimal Balance, decimal betOutcome, OperationError? Error = null);
}
