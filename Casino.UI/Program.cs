using Casino.Core.Services;

namespace Casino.UI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var walletBaseUrl = System.Configuration.ConfigurationManager.AppSettings["WalletBaseUrl"] ?? throw new InvalidOperationException("WalletBaseUrl is missing.");
            var gameBaseUrl = System.Configuration.ConfigurationManager.AppSettings["GameBaseUrl"] ?? throw new InvalidOperationException("GameBaseUrl is missing.");

            var processor = new CommandProcessor(new CasinoService(walletBaseUrl, gameBaseUrl));

            while(true)
            {
                var commandText = Console.ReadLine();

                if (commandText == null) continue;

                commandText = commandText.Trim().ToUpper();

                if (commandText.Contains("EXIT")) break;

                try
                {
                    var commandResult = processor.ProcessUserCommand(commandText).Result;

                    Console.WriteLine(commandResult);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Internal failure occured : " + ex.ToString());
                }
            }
        }
    }
}
