using System;
using System.Collections.Generic;

namespace UtilityBill.Data.Models;

public partial class MeterReading
{
    public int Id { get; set; }

    public int RoomId { get; set; }

    public int ReadingMonth { get; set; }

    public int ReadingYear { get; set; }

    public decimal ElectricReading { get; set; }

    public decimal WaterReading { get; set; }

    public DateTime ReadingDate { get; set; }

    public string? RecordedByUserId { get; set; }

    public virtual User? RecordedByUser { get; set; }

    public virtual Room Room { get; set; } = null!;
}
