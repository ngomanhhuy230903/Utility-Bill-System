using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UtilityBill.Business.Services;
using UtilityBill.Data.Models;
using UtilityBill.Data.Repositories;

namespace UtilityBill.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaintenanceController : ControllerBase
    {
        private readonly IMaintenanceScheduleService _maintenanceScheduleService;

        public MaintenanceController(IMaintenanceScheduleService maintenanceScheduleService)
        {
            _maintenanceScheduleService = maintenanceScheduleService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateSchedule([FromBody] MaintenanceScheduleDTO dto)
        {
            var schedule = new MaintenanceSchedule
            {
                RoomId = dto.RoomId,
                Block = dto.Block,
                Title = dto.Title,
                Description = dto.Description,
                ScheduledStart = dto.ScheduledStart,
                ScheduledEnd = dto.ScheduledEnd,
                Status = "Scheduled",
                CreatedByUserId = "admin-user-guid"
            };

            await _maintenanceScheduleService.Create(schedule);
            return Ok(schedule);
        }

        [HttpGet("calendar")]
        public async Task<IActionResult> GetByMonth(int year, int month)
        {
            var result = await _maintenanceScheduleService.GetForCalendar(month, year);

            return Ok(result);
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetList(int year, int month)
        {
            var result = await _maintenanceScheduleService.GetForMonth(month, year);

            return Ok(result);
        }
    }
}
