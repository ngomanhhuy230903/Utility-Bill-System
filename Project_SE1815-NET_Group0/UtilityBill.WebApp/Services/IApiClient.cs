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
        Task<bool> TriggerInvoiceGenerationAsync();
        Task<byte[]?> GetInvoicePdfAsync(Guid invoiceId);
        // Dùng UserDto đã có trong WebApp/DTOs
        Task<List<UserDto>> GetTenantsAsync();
        Task<bool> AssignTenantAsync(int roomId, AssignTenantDto assignDto);
        Task<UserDto?> RegisterAsync(RegisterDto registerDto);
    }
}