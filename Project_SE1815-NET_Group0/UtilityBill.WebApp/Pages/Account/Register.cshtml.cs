using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using UtilityBill.WebApp.DTOs;
using UtilityBill.WebApp.Services;

namespace UtilityBill.WebApp.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly IApiClient _apiClient;
        public RegisterModel(IApiClient apiClient) { _apiClient = apiClient; }

        [BindProperty]
        public RegisterDto Input { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = await _apiClient.RegisterAsync(Input);

            if (result == null)
            {
                ModelState.AddModelError(string.Empty, "Đăng ký thất bại. Tên đăng nhập hoặc email có thể đã tồn tại.");
                return Page();
            }

            // Tự động đăng nhập cho người dùng sau khi đăng ký thành công
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, result.UserName),
                new Claim("JWToken", result.Token)
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return LocalRedirect("/");
        }
    }
}