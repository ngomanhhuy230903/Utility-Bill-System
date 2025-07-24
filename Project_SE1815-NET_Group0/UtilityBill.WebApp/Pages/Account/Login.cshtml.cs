// File: Pages/Account/Login.cshtml.cs

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json; // Cần cho PostAsJsonAsync
using System.Security.Claims;
using UtilityBill.WebApp.DTOs; // Dùng DTOs của WebApp

namespace UtilityBill.WebApp.Pages.Account
{
    public class LoginModel : PageModel
    {
        // Dùng IHttpClientFactory để tạo một "kết nối" đến API
        private readonly IHttpClientFactory _httpClientFactory;

        public LoginModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Dữ liệu người dùng nhập vào từ form
        [BindProperty]
        public LoginDto Input { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            // 1. Kiểm tra xem người dùng đã nhập đủ thông tin chưa
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // 2. Tạo một client để gọi đến API
            var httpClient = _httpClientFactory.CreateClient("ApiClient");

            // 3. Gửi yêu cầu POST đến endpoint "auth/login" của API cùng với dữ liệu Input
            var response = await httpClient.PostAsJsonAsync("auth/login", Input);

            // 4. Nếu API trả về thành công (status 200 OK)
            if (response.IsSuccessStatusCode)
            {
                // Đọc dữ liệu user và token từ API trả về
                var userDto = await response.Content.ReadFromJsonAsync<UserDto>();

                // 5. TẠO PHIÊN ĐĂNG NHẬP (COOKIE) CHO TRANG WEB
                // Lưu các thông tin quan trọng vào cookie để trang web biết người dùng là ai
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, userDto.UserName), // Tên để hiển thị "Xin chào..."
                    new Claim("FullName", userDto.FullName),
                    new Claim("JWToken", userDto.Token) // QUAN TRỌNG: Lưu lại JWT để dùng cho các lần gọi API sau
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // Thực hiện đăng nhập, tạo cookie
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                // Chuyển hướng về trang chủ
                return LocalRedirect("/MyHomepage");
            }
            else
            {
                // Nếu API báo lỗi (sai pass, sai user...), báo lỗi lại cho người dùng
                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không chính xác.");
                return Page();
            }
        }
    }
}