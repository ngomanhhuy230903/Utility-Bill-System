using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;

namespace UtilityBill.Business.DTOs
{
    public class MeterReadingUploadDto
    {
        [Name("RoomNumber")]
        public string RoomNumber { get; set; } = null!;

        [Name("ElectricityReading")]
        public decimal ElectricReading { get; set; }

        [Name("WaterReading")]
        public decimal WaterReading { get; set; }

        [Name("RecordedDate")]
        public DateTime ReadingDate { get; set; }
    }
}
