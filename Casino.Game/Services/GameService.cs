
namespace Casino.Game.Services
{
    public class GameService : IGameService
    {
        public decimal Bet()
        {
            var r = new Random(100).Next(0,100);

            if (r <= 50) return 0;
            else if (r <= 90) return 2;
            else return new Random(100).Next(3,10);

        }
    }
}
