using Casino.Common;
using Casino.Core.Services;
using System.Globalization;

namespace Casino.UI
{
    internal class CommandProcessor
    {
        private readonly ICasinoService casinoService;

        public CommandProcessor(ICasinoService casinoService)
        {
            this.casinoService = casinoService;
        }

        public async Task<string> ProcessUserCommand(CommandType commandType, decimal amount)
        {
            string ParseOperationError(OperationError error, decimal balance)
            {
                if (error == OperationError.InsufficientFunds)
                {
                    return $"Insufficient funds. Your current balance is : ${balance.ToString("0.00", CultureInfo.InvariantCulture)}";
                }
                else
                if (error == OperationError.InvalidAmount)
                {
                    return "Invalid amount";
                }
                else return "Unknown error";
            }

            string FormatMoney(decimal amount) =>   amount.ToString("0.00", CultureInfo.InvariantCulture);

            if (amount <= 0) return "Invalid amount, it should be > 0";

            string resultMessage = "";

            switch (commandType)
            {
                case CommandType.DEPOSIT:
                    {
                        var result = await casinoService.DepositAsync(amount);

                        if (result.Error != null)
                        {
                            resultMessage = ParseOperationError(result.Error.Value, result.Balance);
                        }
                        else
                        {
                            resultMessage = $"Your deposit of ${FormatMoney(amount)} was successful. Your current balance is : ${FormatMoney(result.Balance)}";
                        }

                        break;
                    }

                case CommandType.WITHDRAW:
                    {
                        var result = await casinoService.WithdrawAsync(amount);

                        if (result.Error != null)
                        {
                            resultMessage = ParseOperationError(result.Error.Value, result.Balance);
                        }
                        else
                        {
                            resultMessage = $"Your withdrawal of ${FormatMoney(amount)} was successful. Your current balance is : ${FormatMoney(result.Balance)}";
                        }
                        break;
                    }
                case CommandType.BET:
                    {
                        if (amount < 1 || amount > 10) return "Invalid bet amount, it should be between $1 and $10";

                        var result = await casinoService.BetAsync(amount);

                        if (result.Error != null)
                        {
                            resultMessage = ParseOperationError(result.Error.Value, result.Balance);
                        }
                        else
                        if (result.betOutcome > 0)
                        {
                            resultMessage = $"Congrats - you won ${FormatMoney(result.betOutcome * amount)}! Your current balance is : ${FormatMoney(result.Balance)}";
                        }
                        else
                        {
                            resultMessage = $"Sorry, but you didn't win! Your current balance is : ${FormatMoney(result.Balance)}";
                        }
                        break;
                    }
                default: resultMessage = "Unknown Command Type"; break;
            }

            return resultMessage;
        }
    }

    internal enum CommandType
    {
        DEPOSIT,
        WITHDRAW,
        BET
    }
}
