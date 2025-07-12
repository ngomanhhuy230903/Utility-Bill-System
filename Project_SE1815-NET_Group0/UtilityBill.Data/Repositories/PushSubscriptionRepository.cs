using Microsoft.EntityFrameworkCore;
using UtilityBill.Data.Context;
using UtilityBill.Data.Models;

namespace UtilityBill.Data.Repositories
{
    public class PushSubscriptionRepository : GenericRepository<PushSubscription>, IPushSubscriptionRepository
    {
        public PushSubscriptionRepository(UtilityBillDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<PushSubscription>> GetActiveSubscriptionsByUserIdAsync(string userId)
        {
            return await _context.Set<PushSubscription>()
                .Where(ps => ps.UserId == userId && ps.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<PushSubscription>> GetAllActiveSubscriptionsAsync()
        {
            return await _context.Set<PushSubscription>()
                .Where(ps => ps.IsActive)
                .ToListAsync();
        }

        public async Task<PushSubscription?> GetByEndpointAsync(string endpoint)
        {
            return await _context.Set<PushSubscription>()
                .FirstOrDefaultAsync(ps => ps.Endpoint == endpoint);
        }

        public async Task DeactivateSubscriptionAsync(string endpoint)
        {
            var subscription = await GetByEndpointAsync(endpoint);
            if (subscription != null)
            {
                subscription.IsActive = false;
                _context.Set<PushSubscription>().Update(subscription);
            }
        }
    }
} 