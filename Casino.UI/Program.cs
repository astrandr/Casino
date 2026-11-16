using Casino.Core.Services;
using System.Globalization;

namespace Casino.UI
{
    internal class Program
    {
        private const string UnknownCommandMessage = "Unknown command. Use: 'deposit 10' or 'withdraw 10' or 'bet 10' or 'exit'";
        private const string InvalidAmountMessage = "Invalid amount.";

        static async Task Main(string[] args)
        {
            var walletBaseUrl = System.Configuration.ConfigurationManager.AppSettings["WalletBaseUrl"] ?? throw new InvalidOperationException("WalletBaseUrl is missing.");
            var gameBaseUrl = System.Configuration.ConfigurationManager.AppSettings["GameBaseUrl"] ?? throw new InvalidOperationException("GameBaseUrl is missing.");

            var processor = new CommandProcessor(new CasinoService(walletBaseUrl, gameBaseUrl));

            while (true)
            {
                var commandText = Console.ReadLine();

                if (commandText == null) break;

                commandText = commandText.Trim();

                if (string.IsNullOrEmpty(commandText)) continue;

                var tokens = commandText.Split(new char[] { ' ', '\t' }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                if (tokens.Length == 1 && string.Equals("EXIT", tokens[0], StringComparison.OrdinalIgnoreCase) ) break;

                var name = Enum.GetNames<CommandType>().FirstOrDefault(name => string.Equals(name, tokens[0], StringComparison.OrdinalIgnoreCase));

                if (tokens.Length != 2 || name is null)
                {
                    Console.WriteLine(UnknownCommandMessage);
                    continue;
                }

                var commandType = Enum.Parse<CommandType>(name);

                if (!decimal.TryParse(tokens[1], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out decimal amount))
                {
                    Console.WriteLine(InvalidAmountMessage);
                    continue;
                }

                try
                {
                    var commandResult = await processor.ProcessUserCommand(commandType, amount);

                    Console.WriteLine(commandResult);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
