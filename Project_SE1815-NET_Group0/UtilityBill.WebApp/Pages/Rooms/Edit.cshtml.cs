using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UtilityBill.WebApp.DTOs;
using UtilityBill.WebApp.Services;
using System.Threading.Tasks;

namespace UtilityBill.WebApp.Pages.Rooms
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly IApiClient _apiClient;

        public EditModel(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [BindProperty]
        public UpdateRoomDto Room { get; set; } = new UpdateRoomDto();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // 1. Gọi API để lấy dữ liệu phòng cũ dựa vào ID
            var roomFromApi = await _apiClient.GetRoomByIdAsync(id);

            // Nếu không tìm thấy phòng, trả về trang 404 Not Found
            if (roomFromApi == null)
            {
                return NotFound();
            }

            // 2. Gán dữ liệu lấy được vào thuộc tính 'Room'.
            // Đây là bước quan trọng để đổ dữ liệu cũ vào form.
            Room = new UpdateRoomDto
            {
                RoomNumber = roomFromApi.RoomNumber,
                Block = roomFromApi.Block,
                Floor = roomFromApi.Floor,
                Area = roomFromApi.Area,
                Price = roomFromApi.Price
            };

            // 3. Trả về trang. Khi trang được render, các thẻ input có asp-for="Room.PropertyName"
            // sẽ tự động lấy giá trị từ đây để hiển thị.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var success = await _apiClient.UpdateRoomAsync(id, Room);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Cập nhật thông tin phòng thất bại.");
                return Page();
            }

            return RedirectToPage("./Index");
        }
    }
}