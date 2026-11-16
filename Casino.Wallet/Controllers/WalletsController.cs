using Casino.Contracts.Game;
using Casino.Contracts.Wallet;
using Casino.Wallet.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace Casino.Wallet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService walletService;
        private readonly ILogger<WalletController> logger;
        private readonly string settlementApiKey;

        public WalletController(IWalletService walletService, IConfiguration configuration, ILogger<WalletController> logger)
        {
            this.walletService = walletService;
            this.logger = logger;
            settlementApiKey = configuration["SettlementApiKey"] ?? throw new InvalidOperationException("SettlementApiKey is missing.");
        }

        [HttpGet]
        [Route("balance")]
        public async Task<decimal> GetBalanceAsync()
        {
            return await walletService.GetBalanceAsync();
        }

        [HttpPost]
        [Route("deposit")]
        public async Task<WalletOperationResult> DepositAsync([FromBody] DepositRequest request)
        {
            return await walletService.DepositAsync(request.Amount);
        }


        [HttpPost]
        [Route("withdraw")]
        public async Task<WalletOperationResult> WithdrawAsync([FromBody] WithdrawRequest request)
        {
            return await walletService.WithdrawAsync(request.Amount);
        }

        [HttpPost]
        [Route("settlebet")]
        public async Task<ActionResult<BetOperationResult>> SettleBetAsync([FromBody] SettleBetRequest request, [FromHeader(Name = "X-Settlement-Key")] string? apiKey)
        {
            if (string.IsNullOrWhiteSpace(settlementApiKey) ||  apiKey is null 
                ||!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(apiKey), Encoding.UTF8.GetBytes(settlementApiKey)))
            {
                logger.LogWarning("Unauthorized call.");
                return Unauthorized();
            }

            try
            {
                return await walletService.SettleBetAsync(request.BetAmount, request.PayoutAmount);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Wallet settlement failed.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
