using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityBill.Business.DTOs;

namespace UtilityBill.Business.Interfaces
{
    public interface IMeterReadingService
    {
        Task<IEnumerable<MeterReadingReadDto>> GetMeterReadingsAsync(int? month = null, int? year = null);
        Task<MeterReadingReadDto?> GetMeterReadingByIdAsync(int id);
        Task<MeterReadingReadDto> CreateMeterReadingAsync(MeterReadingCreateDto dto);
        Task<bool> UpdateMeterReadingAsync(int id, MeterReadingCreateDto dto);
        Task<bool> DeleteMeterReadingAsync(int id);
    }
}
