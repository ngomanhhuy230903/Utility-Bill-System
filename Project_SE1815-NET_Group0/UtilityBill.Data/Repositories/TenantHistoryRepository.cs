// File: UtilityBill.Data/Repositories/TenantHistoryRepository.cs
using Microsoft.EntityFrameworkCore;
using UtilityBill.Data.Context;
using UtilityBill.Data.Models;
using System.Threading.Tasks;

namespace UtilityBill.Data.Repositories
{
    public class TenantHistoryRepository : GenericRepository<TenantHistory>, ITenantHistoryRepository
    {
        public TenantHistoryRepository(UtilityBillDbContext context) : base(context)
        {
        }

        public async Task<TenantHistory?> GetCurrentHistoryByRoomIdAsync(int roomId)
        {
            return await _dbSet.FirstOrDefaultAsync(th => th.RoomId == roomId && th.MoveOutDate == null);
        }
        // Thêm vào TenantHistoryRepository.cs
        public async Task<IEnumerable<TenantHistory>> GetHistoriesByRoomIdAsync(int roomId)
        {
            return await _dbSet
                .Where(th => th.RoomId == roomId)
                .Include(th => th.Tenant) // Lấy kèm thông tin của khách thuê (User)
                .OrderByDescending(th => th.MoveInDate)
                .ToListAsync();
        }
    }
}