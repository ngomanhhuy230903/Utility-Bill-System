// File: UtilityBill.Data/Repositories/IUnitOfWork.cs
namespace UtilityBill.Data.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IRoomRepository RoomRepository { get; }
        IUserRepository UserRepository { get; }
        Task<int> SaveChangesAsync();
        ITenantHistoryRepository TenantHistoryRepository { get; }
    }
}