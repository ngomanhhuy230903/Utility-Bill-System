// File: UtilityBill.WebApp/Services/IApiClient.cs
using UtilityBill.WebApp.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UtilityBill.WebApp.Services
{
    public interface IApiClient
    {
        Task<List<RoomDto>> GetRoomsAsync();
        // Các phương thức khác sẽ được thêm sau
    }
}