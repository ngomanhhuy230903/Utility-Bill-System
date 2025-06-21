using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using UtilityBill.Api.DTOs;
using UtilityBill.Business.DTOs;
using UtilityBill.Business.Interfaces;
using UtilityBill.Data.Models;
using UtilityBill.Data.Repositories;
using System.IdentityModel.Tokens.Jwt;
namespace UtilityBill.Api.Controllers
{
    public class AuthController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IEmailService _emailService;
        private readonly ITokenService _tokenService;

        // Constructor đã được dọn dẹp, không còn UserManager hay IAuthService
        public AuthController(
            IUnitOfWork unitOfWork,
            IPasswordHasher<User> passwordHasher,
            IEmailService emailService,
            ITokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _emailService = emailService;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
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

            var token = await _tokenService.CreateToken(user);
            return new UserDto { UserName = user.UserName, Email = user.Email, FullName = user.FullName, Token = token };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _unitOfWork.UserRepository.FirstOrDefaultAsync(u => u.UserName.ToLower() == loginDto.UserName.ToLower());
            if (user == null || _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginDto.Password) == PasswordVerificationResult.Failed)
            {
                return Unauthorized("Sai tên đăng nhập hoặc mật khẩu.");
            }

            var token = await _tokenService.CreateToken(user);
            return new UserDto { UserName = user.UserName, Email = user.Email, FullName = user.FullName, Token = token };
        }


        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            // Tìm user bằng email, không dùng NormalizedEmail.
            var user = await _unitOfWork.UserRepository.FirstOrDefaultAsync(u => u.Email.ToLower() == forgotPasswordDto.Email.ToLower());

            if (user != null)
            {
                var token = _tokenService.GeneratePasswordResetToken(user);
                // Sửa lại cổng 7xxx cho đúng với WebApp của bạn
                var resetLink = $"https://localhost:7240/Account/ResetPassword?token={token}";
                await _emailService.SendPasswordResetEmailAsync(user.Email, resetLink);
            }

            // Luôn trả về thông báo này để bảo mật
            return Ok(new { message = "Nếu email của bạn tồn tại trong hệ thống, chúng tôi đã gửi một link reset mật khẩu." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            var principal = _tokenService.ValidatePasswordResetToken(resetPasswordDto.Token);
            if (principal == null)
            {
                return BadRequest(new { message = "Token không hợp lệ hoặc đã hết hạn." });
            }

            var userId = principal.FindFirstValue(JwtRegisteredClaimNames.NameId);
            var user = await _unitOfWork.UserRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return BadRequest(new { message = "Người dùng không tồn tại." });
            }

            user.PasswordHash = _passwordHasher.HashPassword(user, resetPasswordDto.Password);
            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { message = "Mật khẩu của bạn đã được thay đổi thành công." });
        }
    }
}