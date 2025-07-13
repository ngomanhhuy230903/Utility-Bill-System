using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UtilityBill.WebApp.DTOs;
using UtilityBill.WebApp.Services;

namespace UtilityBill.WebApp.Pages.Rooms
{
    [Authorize]
    public class HistoryModel : PageModel
    {
        private readonly IApiClient _apiClient;

        public HistoryModel(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public RoomDto? Room { get; set; }
        public List<TenantHistoryDto> Histories { get; set; } = new List<TenantHistoryDto>();

        public async Task<IActionResult> OnGetAsync(int roomId)
        {
            Room = await _apiClient.GetRoomByIdAsync(roomId);
            if (Room == null)
            {
                return NotFound();
            }

            Histories = await _apiClient.GetRoomHistoryAsync(roomId);
            return Page();
        }
    }
}