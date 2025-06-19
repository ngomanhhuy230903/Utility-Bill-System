using System;
using System.Collections.Generic;

namespace UtilityBill.Data.Models;

public partial class MaintenanceSchedule
{
    public int Id { get; set; }

    public int? RoomId { get; set; }

    public string? Block { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime ScheduledStart { get; set; }

    public DateTime ScheduledEnd { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string CreatedByUserId { get; set; } = null!;

    public virtual User CreatedByUser { get; set; } = null!;

    public virtual Room? Room { get; set; }
}
