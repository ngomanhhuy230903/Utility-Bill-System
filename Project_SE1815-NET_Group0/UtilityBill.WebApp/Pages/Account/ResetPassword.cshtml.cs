using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UtilityBill.WebApp.DTOs;
using UtilityBill.WebApp.Services;

namespace UtilityBill.WebApp.Pages.Account
{
    public class ResetPasswordModel : PageModel
    {
        private readonly IApiClient _apiClient;
        public ResetPasswordModel(IApiClient apiClient) { _apiClient = apiClient; }

        [BindProperty]
        public ResetPasswordDto Input { get; set; }

        public IActionResult OnGet(string email = null, string token = null)
        {
            if (email == null || token == null)
            {
                return BadRequest("Link reset mật khẩu không hợp lệ hoặc đã hết hạn.");
            }
            Input = new ResetPasswordDto { Email = email, Token = token };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var success = await _apiClient.ResetPasswordAsync(Input);
            if (success)
            {
                return RedirectToPage("./ResetPasswordConfirmation");
            }

            ModelState.AddModelError(string.Empty, "Không thể reset mật khẩu. Token có thể đã hết hạn hoặc không hợp lệ.");
            return Page();
        }
    }
}