// File: UtilityBill.Data/Repositories/ITenantHistoryRepository.cs
using UtilityBill.Data.Models;
using System.Threading.Tasks;

namespace UtilityBill.Data.Repositories
{
    public interface ITenantHistoryRepository : IGenericRepository<TenantHistory>
    {
        Task<TenantHistory?> GetCurrentHistoryByRoomIdAsync(int roomId);
        Task<IEnumerable<TenantHistory>> GetHistoriesByRoomIdAsync(int roomId);
    }
}