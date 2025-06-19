using System;
using System.Collections.Generic;

namespace UtilityBill.Data.Models;

public partial class User
{
    public string Id { get; set; } = null!;

    public string UserName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<MaintenanceSchedule> MaintenanceSchedules { get; set; } = new List<MaintenanceSchedule>();

    public virtual ICollection<MeterReading> MeterReadings { get; set; } = new List<MeterReading>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<TenantHistory> TenantHistories { get; set; } = new List<TenantHistory>();

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
