using Microsoft.EntityFrameworkCore;
using System;
using UtilityBill.Data.Models;
using UtilityBill.Data.Repositories;
using UtilityBill.Business.Interfaces;
using UtilityBill.Data.DTOs;

namespace UtilityBill.Api.BackgroundServices
{
    public class MaintenanceReminderJob
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPushNotificationService _pushNotificationService;

        public MaintenanceReminderJob(IUnitOfWork unitOfWork, IPushNotificationService pushNotificationService)
        {
            _unitOfWork = unitOfWork;
            _pushNotificationService = pushNotificationService;
        }

        public async Task Execute()
        {
            var tomorrow = DateTime.Today.AddDays(1);
            var items = await _unitOfWork.MaintenanceScheduleRepository.GetByMonthAndStatus(tomorrow, "Scheduled");

            foreach (var item in items)
            {
                // Add to database notifications
                await _unitOfWork.NotificationRepository.AddAsync(new Notification
                {
                    UserId = "admin-user-guid",
                    Type = "PUSH",
                    Content = $"Nhắc nhở: Bảo trì \"{item.Title}\" sẽ diễn ra lúc {item.ScheduledStart:HH:mm dd/MM/yyyy}",
                    RelatedEntityId = item.Id.ToString()
                });

                // Send push notification to admin
                var notification = new PushNotificationDto
                {
                    Title = "Maintenance Reminder",
                    Body = $"Nhắc nhở: Bảo trì \"{item.Title}\" sẽ diễn ra lúc {item.ScheduledStart:HH:mm dd/MM/yyyy}",
                    Tag = "maintenance-reminder",
                    Data = $"{{\"maintenanceId\": \"{item.Id}\", \"url\": \"/Maintenance\"}}"
                };

                await _pushNotificationService.SendNotificationToUserAsync("admin-user-guid", notification);
            }

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
