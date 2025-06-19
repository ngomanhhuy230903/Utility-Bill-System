using System;
using System.Collections.Generic;

namespace UtilityBill.Data.Models;

public partial class TenantHistory
{
    public int Id { get; set; }

    public int RoomId { get; set; }

    public string TenantId { get; set; } = null!;

    public DateOnly MoveInDate { get; set; }

    public DateOnly? MoveOutDate { get; set; }

    public virtual Room Room { get; set; } = null!;

    public virtual User Tenant { get; set; } = null!;
}
