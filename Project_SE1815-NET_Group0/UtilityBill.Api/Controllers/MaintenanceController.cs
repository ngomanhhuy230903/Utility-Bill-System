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

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _maintenanceScheduleService.GetById(id);
            return Ok(data);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateSchedule([FromBody] MaintenanceScheduleDTO dto)
        {
            try
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
            catch (Exception e)
            {
                return BadRequest();
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSchedule([FromBody] MaintenanceScheduleDTO schedule, int? id)
        {

            try
            {
                if (id == null || id <= 0)
                {
                    return BadRequest("Invalid schedule ID");
                }
                var existing = await _maintenanceScheduleService.GetById(id.Value);
                if (existing == null)
                {
                    throw new Exception("Schedule not found");
                }

                // Update fields
                existing.Title = schedule.Title;
                existing.Description = schedule.Description;
                existing.ScheduledStart = schedule.ScheduledStart;
                existing.ScheduledEnd = schedule.ScheduledEnd;
                existing.RoomId = schedule.RoomId;

                await _maintenanceScheduleService.Update(existing);
                return Ok(existing);
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            var existing = await _maintenanceScheduleService.GetById(id);
            if (existing == null)
            {
                return NotFound("Schedule not found");
            }
            await _maintenanceScheduleService.Delete(existing);
            return Ok();
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
