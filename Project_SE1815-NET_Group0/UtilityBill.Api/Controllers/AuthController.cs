using Microsoft.AspNetCore.Mvc;
using UtilityBill.Business.DTOs;
using UtilityBill.Business.Interfaces;

namespace UtilityBill.Api.Controllers
{
    public class AuthController : BaseApiController
    {
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;

        public AuthController(IAuthService authService, ITokenService tokenService)
        {
            _authService = authService;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {   
            var user = await _authService.RegisterAsync(registerDto);
            if (user == null)
            {
                return BadRequest("Username is already taken or invalid.");
            }

            return new UserDto
            {
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                Token = await _tokenService.CreateToken(user)
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _authService.LoginAsync(loginDto);
            if (user == null)
            {
                return Unauthorized("Invalid username or password.");
            }

            return new UserDto
            {
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                Token = await _tokenService.CreateToken(user)
            };
        }
    }
}