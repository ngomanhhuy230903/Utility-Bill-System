using UtilityBill.Data.Models;
namespace UtilityBill.Data.Repositories
{
    public interface IInvoiceRepository : IGenericRepository<Invoice>
    {
        Task<bool> CheckIfInvoiceExistsAsync(int roomId, int year, int month);
        Task<Invoice?> GetInvoiceWithDetailsAsync(Guid invoiceId);
        Task<IEnumerable<Invoice>> GetAllInvoicesWithRoomAsync();
    }
}