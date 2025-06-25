using Microsoft.EntityFrameworkCore;
using System;
using UtilityBill.Data.Models;
using UtilityBill.Data.Repositories;

namespace UtilityBill.Api.BackgroundServices
{
    public class MaintenanceReminderJob
    {
        private readonly IUnitOfWork _unitOfWork;

        public MaintenanceReminderJob(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Execute()
        {
            var tomorrow = DateTime.Today.AddDays(1);
            var items = await _unitOfWork.MaintenanceScheduleRepository.GetByMonthAndStatus(tomorrow, "Scheduled");

            foreach (var item in items)
            {
                await _unitOfWork.NotificationRepository.AddAsync(new Notification
                {
                    UserId = "admin-user-guid",
                    Type = "PUSH",
                    Content = $"Nhắc nhở: Bảo trì \"{item.Title}\" sẽ diễn ra lúc {item.ScheduledStart:HH:mm dd/MM/yyyy}",
                    RelatedEntityId = item.Id.ToString()
                });
            }

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
