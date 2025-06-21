using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UtilityBill.WebApp.DTOs;
using UtilityBill.WebApp.Services;

namespace UtilityBill.WebApp.Pages.Rooms
{
    [Authorize] // Bắt buộc phải đăng nhập để vào trang này
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
            Rooms = await _apiClient.GetRoomsAsync();
        }
    }
}