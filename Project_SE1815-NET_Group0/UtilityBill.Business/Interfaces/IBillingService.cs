using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityBill.Data.Models;

namespace UtilityBill.Business.Interfaces
{
    public interface IBillingService
    {
        Task GenerateInvoicesForPreviousMonthAsync();
        Task<Invoice?> GetInvoiceByIdAsync(Guid invoiceId);
    }
}