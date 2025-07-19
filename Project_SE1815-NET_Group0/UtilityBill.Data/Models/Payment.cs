using UtilityBill.Data.Enums;

namespace UtilityBill.Data.Models;

public partial class Payment
{
    public Guid Id { get; set; }

    public Guid InvoiceId { get; set; }

    public DateTime PaymentDate { get; set; }

    public decimal Amount { get; set; }

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.NONE;

    public string? TransactionCode { get; set; }

    public string Status { get; set; } = null!;

    public string? Notes { get; set; }

    public virtual Invoice Invoice { get; set; } = null!;
}
