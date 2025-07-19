using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UtilityBill.WebApp.DTOs;
using UtilityBill.WebApp.Services;

namespace UtilityBill.WebApp.Pages.Rooms
{
    [Authorize]
    public class UnassignTenantModel : PageModel
    {
        private readonly IApiClient _apiClient;

        public UnassignTenantModel(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [BindProperty]
        public RoomDto? Room { get; set; }

        public async Task<IActionResult> OnGetAsync(int roomId)
        {
            Room = await _apiClient.GetRoomByIdAsync(roomId);
            if (Room == null || Room.Status != "Occupied")
            {
                return NotFound("Phòng không có người ở hoặc không tồn tại.");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int roomId)
        {
            await _apiClient.UnassignTenantAsync(roomId);
            return RedirectToPage("./Index");
        }
    }
}