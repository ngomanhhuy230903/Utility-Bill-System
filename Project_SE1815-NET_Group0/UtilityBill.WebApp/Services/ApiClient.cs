// File: UtilityBill.WebApp/Services/ApiClient.cs
using System.Net.Http.Headers;
using System.Security.Claims;
using UtilityBill.WebApp.DTOs; // <-- SỬA LẠI USING

namespace UtilityBill.WebApp.Services
{
    public class ApiClient : IApiClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        private HttpClient GetAuthenticatedClient()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            // Lấy token đã lưu trong cookie khi đăng nhập
            var token = _httpContextAccessor.HttpContext?.User.FindFirstValue("JWToken");

            if (!string.IsNullOrEmpty(token))
            {
                // Gắn token vào header của mỗi request
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return client;
        }

        public async Task<List<RoomDto>> GetRoomsAsync()
        {
            var client = GetAuthenticatedClient();
            try
            {
                // Gọi đến endpoint "rooms" của API
                var result = await client.GetFromJsonAsync<List<RoomDto>>("rooms");
                return result ?? new List<RoomDto>();
            }
            catch (HttpRequestException) // Xử lý trường hợp API không hoạt động hoặc lỗi
            {
                // Trả về danh sách rỗng nếu có lỗi kết nối
                return new List<RoomDto>();
            }
        }
        // Thêm các phương thức này vào bên trong class ApiClient

        public async Task<RoomDto?> GetRoomByIdAsync(int id)
        {
            var client = GetAuthenticatedClient();
            return await client.GetFromJsonAsync<RoomDto>($"rooms/{id}");
        }

        public async Task<RoomDto?> CreateRoomAsync(CreateRoomDto room)
        {
            var client = GetAuthenticatedClient();
            var response = await client.PostAsJsonAsync("rooms", room);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<RoomDto>();
            }
            return null;
        }

        public async Task<bool> UpdateRoomAsync(int id, UpdateRoomDto room)
        {
            var client = GetAuthenticatedClient();
            var response = await client.PutAsJsonAsync($"rooms/{id}", room);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteRoomAsync(int id)
        {
            var client = GetAuthenticatedClient();
            var response = await client.DeleteAsync($"rooms/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}