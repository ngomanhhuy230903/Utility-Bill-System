using System;
using System.Collections.Generic;

namespace UtilityBill.Data.Models;

public partial class Payment
{
    public Guid Id { get; set; }

    public Guid InvoiceId { get; set; }

    public DateTime PaymentDate { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string? TransactionCode { get; set; }

    public string Status { get; set; } = null!;

    public string? Notes { get; set; }

    public virtual Invoice Invoice { get; set; } = null!;
}
