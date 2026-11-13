
namespace Casino.Game.Services
{
    public class GameService : IGameService
    {
        public decimal Bet()
        {
            var r = Random.Shared.Next(100);

            if (r < 50) return 0m;
            if (r < 90) return Random.Shared.Next(1, 200) / 100m;
            return Random.Shared.Next(201, 1000) / 100m;

        }
    }
}
