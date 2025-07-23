using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityBill.Data.Context;
using UtilityBill.Data.Models;

namespace UtilityBill.Data.Repositories
{
    public class MaintenanceScheduleRepository : GenericRepository<MaintenanceSchedule>, IMaintenanceScheduleRepository
    {
        public MaintenanceScheduleRepository(UtilityBillDbContext context) : base(context)
        {
        }

        public async Task<IList<MaintenanceSchedule>> GetByMonthAndStatus(DateTime date, string status)
        {
            var result = _dbSet.Where(x => x.ScheduledStart.Date == date && x.Status == "Scheduled").ToList();
            return result;
        }

        public async Task<IList<MaintenanceSchedule>> GetByMonthAsync(int month, int year)
        {
            var result = _dbSet
                .Where(s => s.ScheduledStart.Month == month && s.ScheduledStart.Year == year)
                .ToList();

            return result;
        }
    }

    public class MaintenanceScheduleDTO
    {
        public int? RoomId { get; set; }
        public string? Block { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime ScheduledStart { get; set; }
        public DateTime ScheduledEnd { get; set; }
        public string? Status { get; set; }
        public string? CreatedByUserId { get; set; }
    }

    public class CalendarEventDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string? BackgroundColor { get; set; }
    }
}
