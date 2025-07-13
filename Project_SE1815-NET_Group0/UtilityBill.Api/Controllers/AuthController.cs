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
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;
        private readonly ITokenService _tokenService;

        public AuthController(
            IUnitOfWork unitOfWork,
            IAuthService authService,
            IEmailService emailService,
            ITokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _authService = authService;
            _emailService = emailService;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            var user = await _authService.RegisterAsync(registerDto);
            if (user == null)
            {
                return BadRequest("Tên đăng nhập đã được sử dụng.");
            }

            var token = await _tokenService.CreateToken(user);
            return new UserDto { Id = user.Id, UserName = user.UserName, Email = user.Email, FullName = user.FullName, Token = token };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _authService.LoginAsync(loginDto);
            if (user == null)
            {
                return Unauthorized("Sai tên đăng nhập hoặc mật khẩu.");
            }

            var token = await _tokenService.CreateToken(user);
            return new UserDto { Id = user.Id, UserName = user.UserName, Email = user.Email, FullName = user.FullName, Token = token };
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

            // Use BCrypt to hash the new password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(resetPasswordDto.Password);
            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { message = "Mật khẩu của bạn đã được thay đổi thành công." });
        }
    }
}