using System;
using System.Collections.Generic;

namespace UtilityBill.Data.Models;

public partial class Room
{
    public int Id { get; set; }

    public string RoomNumber { get; set; } = null!;

    public string? Block { get; set; }

    public int? Floor { get; set; }

    public decimal Area { get; set; }

    public decimal Price { get; set; }

    public string? QRCodeData { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual ICollection<MaintenanceSchedule> MaintenanceSchedules { get; set; } = new List<MaintenanceSchedule>();

    public virtual ICollection<MeterReading> MeterReadings { get; set; } = new List<MeterReading>();

    public virtual ICollection<TenantHistory> TenantHistories { get; set; } = new List<TenantHistory>();
}
