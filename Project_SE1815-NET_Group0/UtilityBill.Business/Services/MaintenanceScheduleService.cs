using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityBill.Data.Models;
using UtilityBill.Data.Repositories;
using UtilityBill.Business.Interfaces;
using UtilityBill.Data.DTOs;

namespace UtilityBill.Business.Services
{
    public class MaintenanceScheduleService : IMaintenanceScheduleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMaintenanceScheduleRepository _maintenanceScheduleRepository;
        private readonly ILogger<MaintenanceScheduleService> _logger;
        private readonly IPushNotificationService _pushNotificationService;

        public MaintenanceScheduleService(IUnitOfWork unitOfWork, ILogger<MaintenanceScheduleService> logger, IPushNotificationService pushNotificationService)
        {
            _unitOfWork = unitOfWork;
            _maintenanceScheduleRepository = unitOfWork.MaintenanceScheduleRepository;
            _logger = logger;
            _pushNotificationService = pushNotificationService;
        }

        public async Task<int> Create(MaintenanceSchedule schedule)
        {
            await _maintenanceScheduleRepository.AddAsync(schedule);
            var result = await _unitOfWork.SaveChangesAsync();
            
            // Send notification for maintenance schedule creation
            try
            {
                var notification = new PushNotificationDto
                {
                    Title = "Maintenance Scheduled",
                    Body = $"Maintenance '{schedule.Title}' has been scheduled for {schedule.ScheduledStart:dd/MM/yyyy HH:mm}.",
                    Tag = "maintenance-scheduled",
                    Data = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        type = "maintenance_scheduled",
                        maintenanceId = schedule.Id,
                        title = schedule.Title,
                        description = schedule.Description,
                        scheduledStart = schedule.ScheduledStart.ToString("yyyy-MM-dd HH:mm"),
                        scheduledEnd = schedule.ScheduledEnd.ToString("yyyy-MM-dd HH:mm"),
                        roomId = schedule.RoomId
                    })
                };

                await _pushNotificationService.SendNotificationAsync(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send maintenance schedule creation notification");
            }
            
            return result;
        }

        public async Task<int> Update(MaintenanceSchedule schedule)
        {
            _maintenanceScheduleRepository.Update(schedule);
            return await _unitOfWork.SaveChangesAsync();
        }

        public async Task<int> Delete(MaintenanceSchedule schedule)
        {
            _maintenanceScheduleRepository.Delete(schedule);
            return await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IList<MaintenanceSchedule>> GetForMonth(int month, int year)
        {
            return await _maintenanceScheduleRepository.GetByMonthAsync(month, year);
        }   

        public async Task<IList<CalendarEventDTO>> GetForCalendar(int month, int year)
        {
            var temp = await _maintenanceScheduleRepository.GetByMonthAsync(month, year);
            var result = temp.Select(m => new CalendarEventDTO
            {
                Id = m.Id,
                Title = m.Title,
                Start = m.ScheduledStart,
                End = m.ScheduledEnd,
                BackgroundColor = m.Status == "Scheduled" ? "#fbc02d"
                               : m.Status == "InProgress" ? "#29b6f6"
                               : m.Status == "Completed" ? "#66bb6a"
                               : "#ef5350"
            }).ToList();

            return result;
        }

        public async Task<MaintenanceSchedule> GetById(int id)
        {
            return await _maintenanceScheduleRepository.GetByIdAsync(id)
                   ?? throw new KeyNotFoundException($"Maintenance schedule with ID {id} not found.");
        }
    }
}
