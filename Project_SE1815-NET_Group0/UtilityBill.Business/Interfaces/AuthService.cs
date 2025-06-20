// File: UtilityBill.Business/Services/AuthService.cs
using UtilityBill.Business.DTOs;
using UtilityBill.Business.Interfaces;
using UtilityBill.Data.Models;
using UtilityBill.Data.Repositories;
using BCrypt.Net;
namespace UtilityBill.Business.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<User?> LoginAsync(LoginDto loginDto)
        {
            // 1. Tìm user trong DB
            var user = await _unitOfWork.UserRepository.GetByUsernameAsync(loginDto.UserName);
            if (user == null)
            {
                return null; // Không tìm thấy user
            }

            // 2. Dùng BCrypt để kiểm tra mật khẩu
            // So sánh mật khẩu người dùng nhập vào (loginDto.Password) với chuỗi hash trong DB (user.PasswordHash)
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                return null; // Sai mật khẩu
            }

            return user; // Đăng nhập thành công, trả về user
        }

        public async Task<User?> RegisterAsync(RegisterDto registerDto)
        {
            // 1. Kiểm tra username đã tồn tại chưa
            var existingUser = await _unitOfWork.UserRepository.GetByUsernameAsync(registerDto.UserName);
            if (existingUser != null)
            {
                // Có thể throw exception hoặc trả về null để báo lỗi
                return null;
            }

            // 2. Tạo đối tượng User mới
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                FullName = registerDto.FullName,
                PhoneNumber = registerDto.PhoneNumber,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            // 3. Băm mật khẩu bằng BCrypt và lưu vào user
            // Đây là bước mã hóa quan trọng!
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            // 4. Lưu user vào database
            await _unitOfWork.UserRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return user;
        }
    }
}