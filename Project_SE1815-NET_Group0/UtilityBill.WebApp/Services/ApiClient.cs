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
        // Thêm 2 phương thức này vào ApiClient.cs
        public async Task<List<InvoiceDto>> GetInvoicesAsync()
        {
            var client = GetAuthenticatedClient();
            try
            {
                return await client.GetFromJsonAsync<List<InvoiceDto>>("billing");
            }
            catch { return new List<InvoiceDto>(); }
        }

        public async Task<InvoiceDto?> GetInvoiceByIdAsync(Guid invoiceId)
        {
            var client = GetAuthenticatedClient();
            try
            {
                return await client.GetFromJsonAsync<InvoiceDto>($"billing/{invoiceId}");
            }
            catch { return null; }
        }

        public async Task<bool> TriggerInvoiceGenerationAsync()
        {
            var client = GetAuthenticatedClient();
            var response = await client.PostAsync("billing/generate-invoices", null);
            return response.IsSuccessStatusCode;
        }
        // Thêm phương thức này vào trong class ApiClient
        public async Task<byte[]?> GetInvoicePdfAsync(Guid invoiceId)
        {
            var client = GetAuthenticatedClient();
            try
            {
                // Gọi đến API và đọc nội dung trả về dưới dạng một mảng byte
                var pdfBytes = await client.GetByteArrayAsync($"billing/{invoiceId}/pdf");
                return pdfBytes;
            }
            catch (HttpRequestException)
            {
                // Trả về null nếu có lỗi (ví dụ: 404 Not Found)
                return null;
            }
        }
        // Thêm vào ApiClient.cs
        public async Task<List<UserDto>> GetTenantsAsync()
        {
            var client = GetAuthenticatedClient();
            try
            {
                // API trả về List<User>, nhưng các trường cơ bản khớp với UserDto
                return await client.GetFromJsonAsync<List<UserDto>>("users");
            }
            catch { return new List<UserDto>(); }
        }

        public async Task<bool> AssignTenantAsync(int roomId, AssignTenantDto assignDto)
        {
            var client = GetAuthenticatedClient();
            var response = await client.PostAsJsonAsync($"rooms/{roomId}/assign-tenant", assignDto);
            return response.IsSuccessStatusCode;
        }
        public async Task<UserDto?> RegisterAsync(RegisterDto registerDto)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.PostAsJsonAsync("auth/register", registerDto);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserDto>();
            }
            return null;
        }
        public async Task<bool> UnassignTenantAsync(int roomId)
        {
            var client = GetAuthenticatedClient();
            var response = await client.PostAsync($"rooms/{roomId}/unassign-tenant", null);
            return response.IsSuccessStatusCode;
        }
        public async Task<List<TenantHistoryDto>> GetRoomHistoryAsync(int roomId)
        {
            var client = GetAuthenticatedClient();
            try
            {
                return await client.GetFromJsonAsync<List<TenantHistoryDto>>($"rooms/{roomId}/history");
            }
            catch
            {
                return new List<TenantHistoryDto>();
            }
        }
        // Thêm 2 phương thức này vào ApiClient.cs
        public async Task<bool> ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            // Endpoint này không cần xác thực
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.PostAsJsonAsync("auth/forgot-password", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
        {
            // Endpoint này không cần xác thực
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.PostAsJsonAsync("auth/reset-password", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<object?> CreateUnifiedPaymentAsync(object paymentRequest, string paymentMethod)
        {
            var client = GetAuthenticatedClient();
            try
            {
                var response = await client.PostAsJsonAsync($"payments/create?paymentMethod={paymentMethod}", paymentRequest);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<object>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}