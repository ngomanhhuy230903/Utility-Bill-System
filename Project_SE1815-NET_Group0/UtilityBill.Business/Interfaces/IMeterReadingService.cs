using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityBill.Business.DTOs;
using UtilityBill.Data.Repositories;

namespace UtilityBill.Business.Interfaces
{
    public interface IMeterReadingService
    {
        Task<IEnumerable<MeterReadingReadDto>> GetMeterReadingsAsync(int? month = null, int? year = null);
        Task<MeterReadingReadDto?> GetMeterReadingByIdAsync(int id);
        Task<MeterReadingReadDto> CreateMeterReadingAsync(MeterReadingCreateDto dto);
        Task<bool> UpdateMeterReadingAsync(
                int roomId,
                int readingYear,
                int readingMonth,
                MeterReadingUpdateDto dto);
        Task<bool> DeleteMeterReadingAsync(
                int roomId,
                int readingYear,
                int readingMonth);

        // Bulk CSV import
        Task<(int successCount, int failCount)> BulkCreateMeterReadingsAsync(IList<MeterReadingUploadDto> dtos);

        // Lookup by room and period
        Task<MeterReadingReadDto?> GetByRoomAndPeriodAsync(int roomId, int year, int month);

        // Consumption report
        Task<IList<ConsumptionReportDto>> GetConsumptionReportAsync(DateTime from, DateTime to, string groupBy);

    }
}
