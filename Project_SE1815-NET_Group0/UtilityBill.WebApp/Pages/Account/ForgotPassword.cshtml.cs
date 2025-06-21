using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UtilityBill.WebApp.DTOs;
using UtilityBill.WebApp.Services;

namespace UtilityBill.WebApp.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly IApiClient _apiClient;
        public ForgotPasswordModel(IApiClient apiClient) { _apiClient = apiClient; }

        [BindProperty]
        public ForgotPasswordDto Input { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await _apiClient.ForgotPasswordAsync(Input);
            return RedirectToPage("./ForgotPasswordConfirmation");
        }
    }
}