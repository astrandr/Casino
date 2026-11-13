using Casino.Contracts.Wallet;
using Casino.Wallet.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Casino.Wallet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalletsController : ControllerBase
    {
        private readonly IWalletService walletService;

        public WalletsController(IWalletService walletService)
        {
            this.walletService = walletService;
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
            return await walletService.DepositAsync(request.RequestId, request.Amount);
        }


        [HttpPost]
        [Route("withdraw")]
        public async Task<WalletOperationResult> WithdrawAsync([FromBody] WithdrawRequest request)
        {
            return await walletService.WithdrawAsync(request.RequestId, request.Amount);
        }
    }
}
