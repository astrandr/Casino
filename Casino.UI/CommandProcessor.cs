using Casino.Core.Services;

namespace Casino.UI
{
    internal class CommandProcessor
    {
        private readonly ICasinoService casinoService;

        private const string InvalidAmountMessage = "Invalid amount.";
        private const string UnknownCommandMessage = "Unknown command";

        public CommandProcessor(ICasinoService casinoService)
        {
            this.casinoService = casinoService;
        }

        public async Task<string> ProcessUserCommand(string commandText)
        {
            var tokens = commandText.Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.RemoveEmptyEntries);

            if (!Enum.TryParse<CommandType>(tokens[0], out CommandType commandType))
            {
                return UnknownCommandMessage;
            }

            if (tokens.Length < 2 || !decimal.TryParse(tokens[1], out decimal amount))
            {
                return InvalidAmountMessage;
            }

            if (amount < 0) return "Invalid amount, it should be > 0";

            string resultMessage = "";

            switch (commandType)
            {
                case CommandType.DEPOSIT:
                    {
                        var result = await casinoService.DepositAsync(amount);

                        if (result.Error != null && result.Error == Contracts.Wallet.WalletError.InsufficientFunds)
                        {
                            resultMessage = "Insufficient funds.";
                        }
                        else
                        {
                            resultMessage = $"Your deposit of ${amount} was successful. Your current balance is : ${result.Balance}";
                        }

                        break;
                    }

                case CommandType.WITHDRAW:
                    {
                        var result = await casinoService.WithdrawAsync(amount);
                        if (result.Error != null && result.Error == Contracts.Wallet.WalletError.InsufficientFunds)
                        {
                            resultMessage = "Insufficient funds.";
                        }
                        else
                        {
                            resultMessage = $"Your withdrawal of ${amount} was successful. Your current balance is : ${result.Balance}";
                        }
                        break;
                    }
                case CommandType.BET:
                    {
                        if (amount < 1 || amount > 10) return "Invalid bet amount, it should be between $1 and $10";

                        var result = await casinoService.BetAsync(amount);
                        if (result.betOutcome > 0)
                        {
                            resultMessage = $"Congrats - you won ${result.betOutcome}! Your current balance is : ${result.Balance}";
                        }
                        else
                        {
                            resultMessage = $"Sorry, but you didn't win! Your current balance is : ${result.Balance}";
                        }
                        break;
                    }
            }

            return resultMessage;
        }

        internal enum CommandType
        {
            DEPOSIT,
            WITHDRAW,
            BET
        }
    }
}
