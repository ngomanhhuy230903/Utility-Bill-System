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
                // Log that user was not found
                Console.WriteLine($"Login failed: User '{loginDto.UserName}' not found");
                return null; // Không tìm thấy user
            }

            // Log that user was found
            Console.WriteLine($"User '{loginDto.UserName}' found, checking password...");
            Console.WriteLine($"Stored password hash: {user.PasswordHash}");

            // 2. Dùng BCrypt để kiểm tra mật khẩu
            // So sánh mật khẩu người dùng nhập vào (loginDto.Password) với chuỗi hash trong DB (user.PasswordHash)
            bool isPasswordValid = false;
            try
            {
                isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);
                Console.WriteLine($"Password verification result: {isPasswordValid}");
            }
            catch (BCrypt.Net.SaltParseException ex)
            {
                // Password hash is not in valid BCrypt format
                // This could happen if the password was stored as plain text or with a different hashing algorithm
                Console.WriteLine($"BCrypt salt parse error: {ex.Message}");
                Console.WriteLine($"Password hash format is invalid: {user.PasswordHash}");
                return null;
            }
            catch (Exception ex)
            {
                // Any other BCrypt-related error
                Console.WriteLine($"BCrypt error: {ex.Message}");
                return null;
            }

            if (!isPasswordValid)
            {
                Console.WriteLine($"Login failed: Invalid password for user '{loginDto.UserName}'");
                return null; // Sai mật khẩu
            }

            Console.WriteLine($"Login successful for user '{loginDto.UserName}'");
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