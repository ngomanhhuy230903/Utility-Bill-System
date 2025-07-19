using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using UtilityBill.Business.Interfaces;
using UtilityBill.Business.DTOs;
using System.Globalization;
using CsvHelper;
using UtilityBill.Business.DTOs;

namespace UtilityBill.Api.Controllers
{
    public class MeterReadingController : Controller
    {
        private readonly IMeterReadingService _service;

        public MeterReadingController(IMeterReadingService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? month, [FromQuery] int? year)
        {
            var result = await _service.GetMeterReadingsAsync(month, year);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetMeterReadingByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MeterReadingCreateDto dto)
        {
            var created = await _service.CreateMeterReadingAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] MeterReadingCreateDto dto)
        {
            var success = await _service.UpdateMeterReadingAsync(id, dto);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteMeterReadingAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
