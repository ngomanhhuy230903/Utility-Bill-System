using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UtilityBill.WebApp.DTOs;
using UtilityBill.WebApp.Services;

namespace UtilityBill.WebApp.Pages.Rooms
{
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly IApiClient _apiClient;

        public DeleteModel(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [BindProperty]
        public RoomDto Room { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Room = await _apiClient.GetRoomByIdAsync(id);
            if (Room == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var success = await _apiClient.DeleteRoomAsync(id);
            if (!success)
            {
                // Có thể thêm xử lý lỗi ở đây nếu cần
                return Page();
            }
            return RedirectToPage("./Index");
        }
    }
}