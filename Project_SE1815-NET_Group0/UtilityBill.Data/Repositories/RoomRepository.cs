using Microsoft.EntityFrameworkCore;
using UtilityBill.Data.Context;
using UtilityBill.Data.Models;

namespace UtilityBill.Data.Repositories
{
    public class RoomRepository : GenericRepository<Room>, IRoomRepository
    {
        public RoomRepository(UtilityBillDbContext context) : base(context)
        {
        }

        public async Task<Room?> GetByRoomNumberAsync(string roomNumber)
        {
            return await _dbSet.FirstOrDefaultAsync(r => r.RoomNumber == roomNumber);
        }
    }
}