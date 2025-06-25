using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityBill.Data.Repositories;

namespace UtilityBill.Business.Services
{
    public interface IReportService
    {
        public Task<IList<ConsumptionReportDto>> GetConsumptionReport(DateTime from, DateTime to, string groupBy);
        public Task<IList<RevenueReportDto>> GetRevenueReport(DateTime from, DateTime to, string groupBy);
    }
}
