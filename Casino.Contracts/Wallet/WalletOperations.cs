using Casino.Common;

namespace Casino.Contracts.Wallet
{
    public sealed record WalletOperationResult(decimal Balance, OperationError? Error = null);

    public struct DepositRequest
    {
        public decimal Amount { get; set; }
    }

    public struct WithdrawRequest
    {
        public decimal Amount { get; set; }
    }

    public struct SettleBetRequest
    {
        public decimal BetAmount { get; set; }
        public decimal PayoutAmount { get; set; }
    }
}
