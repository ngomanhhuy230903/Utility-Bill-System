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

        [BindProperty(SupportsGet = true)]
        public string Email { get; set; } // Nhận email từ trang trước

        [BindProperty]
        public ResetPasswordWithOtpDto Input { get; set; }

        public void OnGet()
        {
            // Gán email vào Input để gửi đi khi POST
            Input = new ResetPasswordWithOtpDto { Email = this.Email };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var (success, errorMessage) = await _apiClient.ResetPasswordWithOtpAsync(Input);
            if (success)
            {
                return RedirectToPage("./ResetPasswordConfirmation");
            }
            ModelState.AddModelError(string.Empty, errorMessage);
            return Page();
        }
    }
}