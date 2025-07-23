using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityBill.Business.DTOs
{
    public class MeterReadingReadDto
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public int ReadingMonth { get; set; }
        public int ReadingYear { get; set; }
        public decimal ElectricReading { get; set; }
        public decimal WaterReading { get; set; }
        public DateTime ReadingDate { get; set; }
        public string RecordedByUserId { get; set; }
    }
}
