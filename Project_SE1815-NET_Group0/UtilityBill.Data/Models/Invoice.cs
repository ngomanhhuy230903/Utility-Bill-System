using System;
using System.Collections.Generic;

namespace UtilityBill.Data.Models;

public partial class Invoice
{
    public Guid Id { get; set; }

    public int RoomId { get; set; }

    public int InvoicePeriodMonth { get; set; }

    public int InvoicePeriodYear { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime DueDate { get; set; }

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new List<InvoiceDetail>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Room Room { get; set; } = null!;
}
