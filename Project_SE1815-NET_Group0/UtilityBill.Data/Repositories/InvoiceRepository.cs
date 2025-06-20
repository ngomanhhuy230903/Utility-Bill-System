using Microsoft.EntityFrameworkCore;
using UtilityBill.Data.Context;
using UtilityBill.Data.Models;
namespace UtilityBill.Data.Repositories
{
    public class InvoiceRepository : GenericRepository<Invoice>, IInvoiceRepository
    {
        public InvoiceRepository(UtilityBillDbContext context) : base(context) { }

        public async Task<bool> CheckIfInvoiceExistsAsync(int roomId, int year, int month)
        {
            return await _dbSet.AnyAsync(i => i.RoomId == roomId && i.InvoicePeriodYear == year && i.InvoicePeriodMonth == month);
        }
    }
}