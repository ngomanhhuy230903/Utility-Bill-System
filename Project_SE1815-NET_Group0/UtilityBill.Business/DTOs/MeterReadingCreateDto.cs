using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace UtilityBill.Business.DTOs
{
    public class MeterReadingCreateDto
    {
        [Required]
        public int RoomId { get; set; }

        [Required, Range(1, 12)]
        public int ReadingMonth { get; set; }

        [Required]
        public int ReadingYear { get; set; }

        [Required]
        public decimal ElectricReading { get; set; }

        [Required]
        public decimal WaterReading { get; set; }

        [Required]
        public DateTime ReadingDate { get; set; }

        // Ghi nhận bởi user nào (Lấy từ token/authenticated user)
        public string RecordedByUserId { get; set; }
    }
}
