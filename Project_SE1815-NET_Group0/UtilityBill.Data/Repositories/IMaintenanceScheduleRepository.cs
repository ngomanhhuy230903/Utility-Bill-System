using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityBill.Data.Models;

namespace UtilityBill.Data.Repositories
{
    public interface IMaintenanceScheduleRepository : IGenericRepository<MaintenanceSchedule>
    {
        public Task<IList<MaintenanceSchedule>> GetByMonthAsync(int month, int year);
        public Task<IList<MaintenanceSchedule>> GetByMonthAndStatus(DateTime date, string status);
    }
}
