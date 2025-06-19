using System;
using System.Collections.Generic;

namespace UtilityBill.Data.Models;

public partial class Notification
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public string Type { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime SentAt { get; set; }

    public bool IsRead { get; set; }

    public string? RelatedEntityId { get; set; }

    public virtual User User { get; set; } = null!;
}
