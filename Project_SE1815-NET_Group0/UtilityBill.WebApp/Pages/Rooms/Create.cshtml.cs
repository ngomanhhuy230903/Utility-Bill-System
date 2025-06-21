using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UtilityBill.WebApp.DTOs;
using UtilityBill.WebApp.Services;

namespace UtilityBill.WebApp.Pages.Rooms
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly IApiClient _apiClient;

        public CreateModel(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [BindProperty]
        public CreateRoomDto Room { get; set; }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = await _apiClient.CreateRoomAsync(Room);
            if (result == null)
            {
                ModelState.AddModelError(string.Empty, "Tạo phòng thất bại. Số phòng có thể đã tồn tại.");
                return Page();
            }

            return RedirectToPage("./Index");
        }
    }
}