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
        // Thêm vào InvoiceRepository.cs
        public async Task<Invoice?> GetInvoiceWithDetailsAsync(Guid invoiceId)
        {
            return await _dbSet
                .Include(i => i.InvoiceDetails) // Lấy chi tiết hóa đơn
                .Include(i => i.Room)           // Lấy thông tin phòng
                .FirstOrDefaultAsync(i => i.Id == invoiceId);
        }
        // Thêm vào InvoiceRepository.cs
        public async Task<IEnumerable<Invoice>> GetAllInvoicesWithRoomAsync()
        {
            return await _dbSet
                .Include(i => i.Room) // Dùng Include để lấy kèm thông tin của Room
                .OrderByDescending(i => i.InvoicePeriodYear)
                .ThenByDescending(i => i.InvoicePeriodMonth)
                .ToListAsync();
        }
    }
}