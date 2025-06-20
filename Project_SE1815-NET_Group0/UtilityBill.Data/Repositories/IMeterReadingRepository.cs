using UtilityBill.Data.Models;
namespace UtilityBill.Data.Repositories
{
    public interface IMeterReadingRepository : IGenericRepository<MeterReading>
    {
        Task<MeterReading?> GetByRoomAndMonthAsync(int roomId, int year, int month);
    }
}