using MraabtaService.Dto_s;

namespace MraabtaService.Services
{
    public interface IAccountService
    {
        Task<CreditClientDto> GetAccount(string UserId, string Password, string AccountId);
    }
}
