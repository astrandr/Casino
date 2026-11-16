using Casino.Contracts.Game;
using Casino.Game.Services;
using Microsoft.AspNetCore.Mvc;

namespace Casino.Game.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly IBetService betService;
        private readonly ILogger<GameController> logger;

        public GameController(IBetService betService, ILogger<GameController> logger)
        {
            this.betService = betService;
            this.logger = logger;
        }

        [HttpPost]
        [Route("bet")]
        public async Task<BetOperationResult> BetAsync([FromBody] BetRequest request)
        {
            logger.LogInformation("Bet called.");
            return await betService.PlaceBetAsync(request.BetAmount);
        }
    }
}
