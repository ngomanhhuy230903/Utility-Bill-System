using UtilityBill.Business.Interfaces;

namespace UtilityBill.Api.BackgroundServices
{
    public class MonthlyBillingJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MonthlyBillingJob> _logger;

        public MonthlyBillingJob(IServiceProvider serviceProvider, ILogger<MonthlyBillingJob> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Chạy vào lúc 2 giờ sáng mỗi ngày
                var now = DateTime.UtcNow;
                var nextRun = now.Date.AddDays(1).AddHours(2);
                var delay = nextRun - now;

                await Task.Delay(delay, stoppingToken);

                // Chỉ thực sự chạy logic vào ngày 5 hàng tháng
                if (DateTime.UtcNow.Day == 5)
                {
                    _logger.LogInformation("Hôm nay là ngày 5, bắt đầu chạy Billing Service...");

                    // Tạo một scope riêng để lấy service, đây là best practice cho background job
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var billingService = scope.ServiceProvider.GetRequiredService<IBillingService>();
                        await billingService.GenerateInvoicesForPreviousMonthAsync();
                    }
                }
            }
        }
    }
}