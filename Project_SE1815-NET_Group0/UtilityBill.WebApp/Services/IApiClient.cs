using UtilityBill.WebApp.DTOs;
namespace UtilityBill.WebApp.Services
{
    public interface IApiClient
    {
        Task<List<RoomDto>> GetRoomsAsync();
        Task<RoomDto?> GetRoomByIdAsync(int id); // Thêm mới
        Task<RoomDto?> CreateRoomAsync(CreateRoomDto room); // Thêm mới
        Task<bool> UpdateRoomAsync(int id, UpdateRoomDto room); // Thêm mới
        Task<bool> DeleteRoomAsync(int id); // Thêm mới
        Task<List<InvoiceDto>> GetInvoicesAsync();
        Task<InvoiceDto?> GetInvoiceByIdAsync(Guid invoiceId); // Thêm mới
        Task<bool> TriggerInvoiceGenerationAsync();
        Task<byte[]?> GetInvoicePdfAsync(Guid invoiceId);
        Task<object?> CreateUnifiedPaymentAsync(object paymentRequest, string paymentMethod); // Thêm mới
        // Dùng UserDto đã có trong WebApp/DTOs
        Task<List<UserDto>> GetTenantsAsync();
        Task<bool> AssignTenantAsync(int roomId, AssignTenantDto assignDto);
        Task<UserDto?> RegisterAsync(RegisterDto registerDto);
        Task<bool> UnassignTenantAsync(int roomId);
        Task<List<TenantHistoryDto>> GetRoomHistoryAsync(int roomId);
        Task<bool> ForgotPasswordAsync(ForgotPasswordDto dto);
        Task<bool> ResetPasswordAsync(ResetPasswordDto dto);
    }
}