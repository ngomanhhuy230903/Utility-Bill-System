using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using UtilityBill.Api.DTOs;
using UtilityBill.Business.DTOs;
using UtilityBill.Business.Interfaces;
using UtilityBill.Data.Models;
using UtilityBill.Data.Repositories;
using UtilityBill.Data.DTOs;

namespace UtilityBill.Api.Controllers
{
    public class AuthController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IEmailService _emailService;
        private readonly IMemoryCache _memoryCache;
        private readonly ITokenService _tokenService;
        private readonly IPushNotificationService _pushNotificationService;

        // Constructor đã được dọn dẹp và thống nhất
        public AuthController(
            IUnitOfWork unitOfWork,
            IPasswordHasher<User> passwordHasher,
            IEmailService emailService,
            IMemoryCache memoryCache,
            ITokenService tokenService,
            IPushNotificationService pushNotificationService)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _emailService = emailService;
            _memoryCache = memoryCache;
            _tokenService = tokenService;
            _pushNotificationService = pushNotificationService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register([FromBody] RegisterDto registerDto)
        {
            var existingUser = await _unitOfWork.UserRepository.FirstOrDefaultAsync(u => u.UserName.ToLower() == registerDto.UserName.ToLower());
            if (existingUser != null)
            {
                return BadRequest("Tên đăng nhập đã được sử dụng.");
            }
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                FullName = registerDto.FullName,
                PhoneNumber = registerDto.PhoneNumber,
                PasswordHash = _passwordHasher.HashPassword(null, registerDto.Password),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            await _unitOfWork.UserRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();
            
            // Send notification for user registration
            try
            {
                var notification = new PushNotificationDto
                {
                    Title = "New User Registered",
                    Body = $"User {user.FullName} ({user.UserName}) has been registered successfully.",
                    Tag = "user-registered",
                    Data = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        type = "user_registered",
                        userId = user.Id,
                        userName = user.UserName,
                        fullName = user.FullName,
                        email = user.Email
                    })
                };

                await _pushNotificationService.SendNotificationAsync(notification);
            }
            catch (Exception ex)
            {
                // Log error but don't fail the registration
                System.Diagnostics.Debug.WriteLine($"Failed to send user registration notification: {ex.Message}");
            }
            
            var token = await _tokenService.CreateToken(user);
            return new UserDto { UserName = user.UserName, Email = user.Email, FullName = user.FullName, Token = token };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login([FromBody] LoginDto loginDto)
        {
            var user = await _unitOfWork.UserRepository.FirstOrDefaultAsync(u => u.UserName.ToLower() == loginDto.UserName.ToLower());
            if (user == null || _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginDto.Password) == PasswordVerificationResult.Failed)
            {
                return Unauthorized("Sai tên đăng nhập hoặc mật khẩu.");
            }
            var token = await _tokenService.CreateToken(user);
            return new UserDto { UserName = user.UserName, Email = user.Email, FullName = user.FullName, Token = token };
        }

        [HttpPost("send-reset-otp")]
        public async Task<IActionResult> SendResetOtp([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            var user = await _unitOfWork.UserRepository.FirstOrDefaultAsync(u => u.Email.ToLower() == forgotPasswordDto.Email.ToLower());
            if (user != null)
            {
                var otp = new Random().Next(100000, 999999).ToString();
                var cacheEntry = new { Otp = otp, UserId = user.Id, Attempts = 0 };
                _memoryCache.Set(user.Email, cacheEntry, TimeSpan.FromMinutes(10));
                await _emailService.SendOtpEmailAsync(user.Email, otp);
            }
            return Ok(new { message = "Nếu email tồn tại, một mã OTP đã được gửi." });
        }

        [HttpPost("reset-password-with-otp")]
        public async Task<IActionResult> ResetPasswordWithOtp([FromBody] ResetPasswordWithOtpDto dto)
        {
            if (!_memoryCache.TryGetValue(dto.Email, out dynamic cacheEntry))
            {
                return BadRequest(new { message = "Mã OTP không hợp lệ hoặc đã hết hạn." });
            }
            if (cacheEntry.Attempts >= 5)
            {
                return BadRequest(new { message = "Bạn đã nhập sai quá nhiều lần. Vui lòng yêu cầu mã OTP mới." });
            }
            if (cacheEntry.Otp != dto.Otp)
            {
                var newEntry = new { Otp = cacheEntry.Otp, UserId = cacheEntry.UserId, Attempts = cacheEntry.Attempts + 1 };
                _memoryCache.Set(dto.Email, newEntry, TimeSpan.FromMinutes(10));
                return BadRequest(new { message = $"Mã OTP không chính xác. Bạn còn {5 - newEntry.Attempts} lần thử." });
            }

            var user = await _unitOfWork.UserRepository.GetUserByIdAsync(cacheEntry.UserId);
            if (user == null)
            {
                return NotFound("Không tìm thấy người dùng.");
            }

            user.PasswordHash = _passwordHasher.HashPassword(user, dto.NewPassword);
            _unitOfWork.UserRepository.Update(user);

            // Dòng này bây giờ sẽ hoạt động đúng nhờ việc sửa GenericRepository
            await _unitOfWork.SaveChangesAsync();

            _memoryCache.Remove(dto.Email);
            return Ok(new { message = "Mật khẩu đã được thay đổi thành công." });
        }
    }
}