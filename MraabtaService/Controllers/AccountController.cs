using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MraabtaService.Dto_s;
using MraabtaService.Services;

namespace MraabtaService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private static IAccountService _accountService;
        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;

        }

        [HttpGet]
        public async Task<CreditClientDto> GetAccountInfo(string? UserId,string? Password,string AccountId)
        {
            return await _accountService.GetAccount(UserId, Password, AccountId);
;
        }
    }
}
