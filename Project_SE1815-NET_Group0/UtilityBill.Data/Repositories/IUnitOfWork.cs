// File: UtilityBill.Data/Repositories/IUnitOfWork.cs
namespace UtilityBill.Data.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IRoomRepository RoomRepository { get; }
        IUserRepository UserRepository { get; }
        Task<int> SaveChangesAsync();
        ITenantHistoryRepository TenantHistoryRepository { get; }
        IMeterReadingRepository MeterReadingRepository { get; }
        IInvoiceRepository InvoiceRepository { get; }
        IPaymentRepository PaymentRepository { get; }
        IMaintenanceScheduleRepository MaintenanceScheduleRepository { get; }
        INotificationRepository NotificationRepository { get; }
    }
}