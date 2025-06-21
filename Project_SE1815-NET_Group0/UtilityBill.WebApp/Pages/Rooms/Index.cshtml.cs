using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UtilityBill.WebApp.DTOs;
using UtilityBill.WebApp.Services;

namespace UtilityBill.WebApp.Pages.Rooms
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IApiClient _apiClient;

        public IndexModel(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public List<RoomDto> Rooms { get; set; } = new List<RoomDto>();

        public async Task OnGetAsync()
        {
            // Lấy dữ liệu từ API và gán vào Rooms để View có thể hiển thị
            Rooms = await _apiClient.GetRoomsAsync();
        }
    }
}