using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityBill.Data.Context;
using UtilityBill.Data.Models;

namespace UtilityBill.Data.Repositories
{
    public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(UtilityBillDbContext context) : base(context)
        {
        }

        public async Task<IList<RevenueReportDto>> GetRevenueReport(DateTime from, DateTime to, string groupBy)
        {
            var list = new List<RevenueReportDto>();
            var query = _dbSet.Where(p => p.PaymentDate >= from && p.PaymentDate <= to && p.Status == "Success");

            switch (groupBy.ToLower())
            {
                case "day":
                    list = query.GroupBy(p => p.PaymentDate.Date)
                        .Select(g => new RevenueReportDto
                        {
                            Period = g.Key.ToString("dd/MM/yyyy"),
                            TotalRevenue = g.Sum(p => p.Amount)
                        }).ToList();
                    return list;

                case "month":
                    list = query.GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
                        .Select(g => new RevenueReportDto
                        {
                            Period = $"{g.Key.Month:00}/{g.Key.Year}",
                            TotalRevenue = g.Sum(p => p.Amount)
                        }).ToList();
                    return list;

                case "year":
                    list = query.GroupBy(p => p.PaymentDate.Year)
                        .Select(g => new RevenueReportDto
                        {
                            Period = g.Key.ToString(),
                            TotalRevenue = g.Sum(p => p.Amount)
                        }).ToList();
                    return list;

                default:
                    throw new ArgumentException("Invalid groupBy");
            }
        }
    }

    public class RevenueReportDto
    {
        public string? Period { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
