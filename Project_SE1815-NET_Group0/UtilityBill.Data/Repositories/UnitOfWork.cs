
using UtilityBill.Data.Context;

namespace UtilityBill.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly UtilityBillDbContext _context;
        private IRoomRepository? _roomRepository;

        public UnitOfWork(UtilityBillDbContext context)
        {
            _context = context;
        }

        public IRoomRepository RoomRepository => _roomRepository ??= new RoomRepository(_context);

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