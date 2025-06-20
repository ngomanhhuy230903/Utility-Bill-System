// File: UtilityBill.Data/Repositories/UnitOfWork.cs
using UtilityBill.Data.Context;

namespace UtilityBill.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly UtilityBillDbContext _context;
        private IRoomRepository? _roomRepository;
        private IUserRepository? _userRepository; // <-- THÊM DÒNG NÀY
        private ITenantHistoryRepository? _tenantHistoryRepository;
        private IMeterReadingRepository? _meterReadingRepository;
        private IInvoiceRepository? _invoiceRepository;
        public UnitOfWork(UtilityBillDbContext context)
        {
            _context = context;
        }

        public IRoomRepository RoomRepository => _roomRepository ??= new RoomRepository(_context);

        // VÀ THÊM CẢ KHỐI NÀY
        public IUserRepository UserRepository => _userRepository ??= new UserRepository(_context);

        public ITenantHistoryRepository TenantHistoryRepository => _tenantHistoryRepository ??= new TenantHistoryRepository(_context);
        public IMeterReadingRepository MeterReadingRepository => _meterReadingRepository ??= new MeterReadingRepository(_context);
        public IInvoiceRepository InvoiceRepository => _invoiceRepository ??= new InvoiceRepository(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}