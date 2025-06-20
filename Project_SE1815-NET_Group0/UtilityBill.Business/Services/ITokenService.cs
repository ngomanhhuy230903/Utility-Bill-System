using UtilityBill.Data.Models;

namespace UtilityBill.Business.Interfaces
{
    public interface ITokenService
    {
        Task<string> CreateToken(User user);
    }
}