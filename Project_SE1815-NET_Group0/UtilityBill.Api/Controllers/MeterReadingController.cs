using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using UtilityBill.Business.Interfaces;
using UtilityBill.Business.DTOs;
using System.Globalization;
using CsvHelper;
using UtilityBill.Business.DTOs;
using CsvHelper.Configuration;

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
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? month,
            [FromQuery] int? year,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            // 1) Retrieve filtered list
            var all = await _service.GetMeterReadingsAsync(month, year);

            // 2) Compute total before paging
            var totalCount = all.Count();

            // 3) Apply paging
            var items = all
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // 4) Return
            return Ok(new
            {
                items,
                totalCount
            });
        }

        [HttpPost("bulk-upload")]
        public async Task<IActionResult> BulkUpload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Please upload a non-empty CSV file.");

            List<MeterReadingUploadDto> records;
            try
            {
                using var stream = file.OpenReadStream();
                using var reader = new StreamReader(stream);
                using var csv = new CsvReader(reader,
                    new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        MissingFieldFound = null,
                        HeaderValidated = null
                    });
                records = csv.GetRecords<MeterReadingUploadDto>().ToList();
            }
            catch (CsvHelper.HeaderValidationException)
            {
                return BadRequest("CSV header invalid. Expected: RoomNumber,ElectricityReading,WaterReading,RecordedDate");
            }
            catch
            {
                return BadRequest("Failed to parse CSV. Check file format and data types.");
            }

            var (successCount, failCount) = await _service.BulkCreateMeterReadingsAsync(records);
            return Ok(new
            {
                total = records.Count,
                success = successCount,
                failed = failCount
            });
        }


    }

}
