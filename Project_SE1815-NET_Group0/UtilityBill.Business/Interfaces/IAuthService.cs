// File: UtilityBill.Business/Interfaces/IAuthService.cs
using UtilityBill.Business.DTOs;
using UtilityBill.Data.Models;

namespace UtilityBill.Business.Interfaces
{
    public interface IAuthService
    {
        // Trả về User nếu đăng ký thành công, null nếu thất bại
        Task<User?> RegisterAsync(RegisterDto registerDto);

        // Trả về User nếu đăng nhập thành công, null nếu thất bại
        Task<User?> LoginAsync(LoginDto loginDto);
    }
}