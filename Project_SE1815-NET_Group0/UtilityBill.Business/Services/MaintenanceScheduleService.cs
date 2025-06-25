using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityBill.Data.Models;
using UtilityBill.Data.Repositories;

namespace UtilityBill.Business.Services
{
    public class MaintenanceScheduleService : IMaintenanceScheduleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMaintenanceScheduleRepository _maintenanceScheduleRepository;
        private readonly ILogger<MaintenanceScheduleService> _logger;

        public MaintenanceScheduleService(IUnitOfWork unitOfWork, ILogger<MaintenanceScheduleService> logger)
        {
            _unitOfWork = unitOfWork;
            _maintenanceScheduleRepository = unitOfWork.MaintenanceScheduleRepository;
            _logger = logger;
        }

        public async Task<int> Create(MaintenanceSchedule schedule)
        {
            await _maintenanceScheduleRepository.AddAsync(schedule);
            return await _unitOfWork.SaveChangesAsync();
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
    }
}
