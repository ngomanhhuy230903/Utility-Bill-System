using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityBill.Data.Repositories
{
    public interface IPaymentRepository
    {
        Task<IList<RevenueReportDto>> GetRevenueReport(DateTime from, DateTime to, string groupBy);
    }
}
