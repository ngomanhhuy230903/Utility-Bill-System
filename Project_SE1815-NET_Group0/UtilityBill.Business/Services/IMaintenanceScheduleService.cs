using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityBill.Data.Models;
using UtilityBill.Data.Repositories;

namespace UtilityBill.Business.Services
{
    public interface IMaintenanceScheduleService
    {
        public Task<int> Create(MaintenanceSchedule schedule);
        public Task<int> Update(MaintenanceSchedule schedule);
        public Task<int> Delete(MaintenanceSchedule schedule);
        public Task<IList<CalendarEventDTO>> GetForCalendar(int month, int year);
        public Task<IList<MaintenanceSchedule>> GetForMonth(int month, int year);       
    }
}
