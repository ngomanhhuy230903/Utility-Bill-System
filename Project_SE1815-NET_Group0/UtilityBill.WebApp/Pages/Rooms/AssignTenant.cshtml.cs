using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using UtilityBill.WebApp.DTOs;
using UtilityBill.WebApp.Services;

namespace UtilityBill.WebApp.Pages.Rooms
{
    [Authorize]
    public class AssignTenantModel : PageModel
    {
        private readonly IApiClient _apiClient;

        public AssignTenantModel(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [BindProperty(SupportsGet = true)]
        public int RoomId { get; set; }
        public RoomDto? Room { get; set; }

        [BindProperty]
        public AssignTenantDto AssignDto { get; set; } = new AssignTenantDto();

        public SelectList TenantList { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            Room = await _apiClient.GetRoomByIdAsync(RoomId);
            if (Room == null || Room.Status != "Vacant")
            {
                return NotFound("Phòng không tồn tại hoặc đã có người ở.");
            }

            var tenants = await _apiClient.GetTenantsAsync();
            TenantList = new SelectList(tenants, "Id", "FullName");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // Nếu lỗi, cần load lại TenantList để hiển thị lại form
                var tenants = await _apiClient.GetTenantsAsync();
                TenantList = new SelectList(tenants, "Id", "FullName");
                Room = await _apiClient.GetRoomByIdAsync(RoomId);
                return Page();
            }

            var success = await _apiClient.AssignTenantAsync(RoomId, AssignDto);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Gán khách thuê thất bại.");
                return await OnGetAsync(); // Tải lại dữ liệu cho form
            }

            return RedirectToPage("./Index");
        }
    }
}