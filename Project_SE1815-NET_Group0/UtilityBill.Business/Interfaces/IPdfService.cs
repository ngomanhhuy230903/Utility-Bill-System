using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityBill.Business.Interfaces
{
    public interface IPdfService
    {
        Task<byte[]?> GenerateInvoicePdfAsync(Guid invoiceId);
    }
}