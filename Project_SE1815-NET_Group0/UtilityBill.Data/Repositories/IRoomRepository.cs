
using UtilityBill.Data.Models;

namespace UtilityBill.Data.Repositories
{
    public interface IRoomRepository : IGenericRepository<Room>
    {
        Task<Room?> GetByRoomNumberAsync(string roomNumber);
    }
}