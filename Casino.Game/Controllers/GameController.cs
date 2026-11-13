using Casino.Game.Services;
using Microsoft.AspNetCore.Mvc;

namespace Casino.Game.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly IGameService gameService;
        private readonly ILogger<GameController> logger;

        public GameController(IGameService gameService, ILogger<GameController> logger)
        {
            this.gameService = gameService;
            this.logger = logger;
        }

        [HttpGet]
        [Route("bet")]
        public decimal Bet()
        {
            logger.LogInformation("Bet called.");
            return gameService.Bet();
        }
    }
}
