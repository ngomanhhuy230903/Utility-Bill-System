using System;
using System.Collections.Generic;

namespace UtilityBill.Data.Models;

public partial class InvoiceDetail
{
    public int Id { get; set; }

    public Guid InvoiceId { get; set; }

    public string Description { get; set; } = null!;

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal Amount { get; set; }

    public virtual Invoice Invoice { get; set; } = null!;
}
