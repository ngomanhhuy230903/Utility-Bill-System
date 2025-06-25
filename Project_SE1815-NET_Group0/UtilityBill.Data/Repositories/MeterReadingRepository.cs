using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
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

        public async Task<IList<ConsumptionReportDto>> GetConsumptionReport(DateTime from, DateTime to, string groupBy)
        {
            var list = new List<ConsumptionReportDto>();
            var query = _dbSet.Where(r => r.ReadingDate >= from && r.ReadingDate <= to);

            switch (groupBy.ToLower())
            {
                case "day":
                    list = query.GroupBy(r => r.ReadingDate.Date)
                        .Select(g => new ConsumptionReportDto
                        {
                            Period = g.Key.ToString("dd/MM/yyyy"),
                            TotalElectric = g.Sum(x => x.ElectricReading),
                            TotalWater = g.Sum(x => x.WaterReading)
                        }).ToList();
                    return list;

                case "month":
                    list = query.GroupBy(r => new { r.ReadingYear, r.ReadingMonth })
                        .Select(g => new ConsumptionReportDto
                        {
                            Period = $"{g.Key.ReadingMonth:00}/{g.Key.ReadingYear}",
                            TotalElectric = g.Sum(x => x.ElectricReading),
                            TotalWater = g.Sum(x => x.WaterReading)
                        }).ToList();
                    return list;

                case "year":
                    list = query.GroupBy(r => r.ReadingYear)
                        .Select(g => new ConsumptionReportDto
                        {
                            Period = g.Key.ToString(),
                            TotalElectric = g.Sum(x => x.ElectricReading),
                            TotalWater = g.Sum(x => x.WaterReading)
                        }).ToList();
                    return list;

                default:
                    throw new ArgumentException("Invalid groupBy");
            }
        }
    }

    public class ConsumptionReportDto
    {
        public string? Period { get; set; }
        public decimal TotalElectric { get; set; }
        public decimal TotalWater { get; set; }
    }
}