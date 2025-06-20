using UtilityBill.Business.DTOs;
using UtilityBill.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UtilityBill.Business.Services
{
    public interface IRoomService
    {
        Task<IEnumerable<Room>> GetAllRoomsAsync();
        Task<Room?> GetRoomByIdAsync(int id);
        Task<Room> CreateRoomAsync(CreateRoomDto roomDto);
        Task<bool> UpdateRoomAsync(int roomId, CreateRoomDto roomDto);
        Task<bool> DeleteRoomAsync(int id);
    }
}