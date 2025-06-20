using Microsoft.EntityFrameworkCore;
using UtilityBill.Data.Context;
using UtilityBill.Data.Models;
namespace UtilityBill.Data.Repositories
{
    public class MeterReadingRepository : GenericRepository<MeterReading>, IMeterReadingRepository
    {
        public MeterReadingRepository(UtilityBillDbContext context) : base(context) { }

        public async Task<MeterReading?> GetByRoomAndMonthAsync(int roomId, int year, int month)
        {
            return await _dbSet.FirstOrDefaultAsync(mr => mr.RoomId == roomId && mr.ReadingYear == year && mr.ReadingMonth == month);
        }
    }
}