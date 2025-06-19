
namespace UtilityBill.Data.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IRoomRepository RoomRepository { get; }

        Task<int> SaveChangesAsync();
    }
}