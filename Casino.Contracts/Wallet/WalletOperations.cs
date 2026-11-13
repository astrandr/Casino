namespace Casino.Contracts.Wallet
{
    public sealed record WalletOperationResult(decimal Balance, Guid requestId, WalletError? Error = null);

    public sealed record BetOperationResult(decimal Balance, decimal betOutcome, WalletError? Error = null);

    public enum WalletError
    {
        InvalidAmount,
        InsufficientFunds
    }

    public struct DepositRequest
    {
        public string RequestId { get; set; }
        public decimal Amount { get; set; }
    }

    public struct WithdrawRequest
    {
        public string RequestId { get; set; }
        public decimal Amount { get; set; }
    }

    public struct SettleBetRequest
    {
        public string RequestId { get; set; }
        public decimal BetAmount { get; set; }
        public decimal PayoutAmount { get; set; }
    }
}
