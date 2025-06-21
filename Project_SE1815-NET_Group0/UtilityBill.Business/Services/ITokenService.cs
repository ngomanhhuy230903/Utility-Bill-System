using UtilityBill.Data.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace UtilityBill.Business.Interfaces
{
    public interface ITokenService
    {
        Task<string> CreateToken(User user); // Token đăng nhập
        string GeneratePasswordResetToken(User user); // Token reset
        ClaimsPrincipal? ValidatePasswordResetToken(string token); // Hàm xác thực token reset
    }
}