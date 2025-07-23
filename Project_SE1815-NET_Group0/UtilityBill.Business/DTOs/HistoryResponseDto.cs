using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityBill.Business.DTOs
{
    public class HistoryResponseDto
    {
        public List<MeterReadingReadDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
