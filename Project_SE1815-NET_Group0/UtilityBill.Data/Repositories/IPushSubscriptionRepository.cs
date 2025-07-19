using UtilityBill.Data.Models;

namespace UtilityBill.Data.Repositories
{
    public interface IPushSubscriptionRepository : IGenericRepository<PushSubscription>
    {
        Task<IEnumerable<PushSubscription>> GetActiveSubscriptionsByUserIdAsync(string userId);
        Task<IEnumerable<PushSubscription>> GetAllActiveSubscriptionsAsync();
        Task<PushSubscription?> GetByEndpointAsync(string endpoint);
        Task DeactivateSubscriptionAsync(string endpoint);
    }
} 