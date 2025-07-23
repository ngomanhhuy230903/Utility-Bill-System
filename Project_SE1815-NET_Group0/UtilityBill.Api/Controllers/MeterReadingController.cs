using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UtilityBill.Business.DTOs;
using UtilityBill.Business.Interfaces;
using UtilityBill.Business.Services;

namespace UtilityBill.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MeterReadingController : ControllerBase
    {
        private readonly IMeterReadingService _service;

        public MeterReadingController(IMeterReadingService service)
            => _service = service;

        /// <summary>
        /// 1. Manual create
        /// POST /api/meterreading/create-reading
        /// Body: MeterReadingCreateDto
        /// </summary>
        [HttpPost("create-reading")]    
        public async Task<IActionResult> CreateReading([FromBody] MeterReadingCreateDto dto)
        {
            var created = await _service.CreateMeterReadingAsync(dto);
            return CreatedAtAction(nameof(GetByRoom), new { roomId = created.RoomId }, created);
        }

        /// <summary>
        /// 2. Get all readings
        /// GET /api/meterreading/get-all
        /// </summary>
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllReadings()
        {
            var all = await _service.GetMeterReadingsAsync(null, null);
            return Ok(all);
        }

        /// <summary>
        /// 3. Get a reading by room & period
        /// GET /api/meterreading/get-by-room/{roomId}?year=&month=
        /// </summary>
        [HttpGet("get-by-room/{roomId:int}")]
        public async Task<IActionResult> GetByRoom(
            int roomId,
            [FromQuery] int year,
            [FromQuery] int month)
        {
            var dto = await _service.GetByRoomAndPeriodAsync(roomId, year, month);
            if (dto is null) return NotFound();
            return Ok(dto);
        }

        /// <summary>
        /// 4. Update a reading by its Id
        /// PUT /api/meterreading/update/{id}
        /// Body: MeterReadingCreateDto
        /// </summary>
        [HttpPut("update")]
        public async Task<IActionResult> UpdateMeterReading(
                [FromQuery] int roomId,
                [FromQuery] int year,
                [FromQuery] int month,
                [FromBody] MeterReadingUpdateDto dto)
        {
            var success = await _service.UpdateMeterReadingAsync(roomId, year, month, dto);
            return success ? Ok() : NotFound();
        }
        /// <summary>
        /// 5. Bulk CSV upload
        /// POST /api/meterreading/bulk-upload
        /// Form‑data: file (.csv)
        /// </summary>
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
            catch (HeaderValidationException)
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

        /// <summary>
        /// 6. History with filter & paging
        /// GET /api/meterreading/history?month=&year=&page=&pageSize=
        /// </summary>
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory(
            [FromQuery] int? month,
            [FromQuery] int? year,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            // 1) retrieve all matching
            var all = await _service.GetMeterReadingsAsync(month, year);

            // 2) page & count
            var totalCount = all.Count();
            var items = all
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new
            {
                items,
                totalCount
            });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteByCompositeKey(
        [FromQuery] int roomId,
        [FromQuery] int year,
        [FromQuery] int month)
        {
            var deleted = await _service.DeleteMeterReadingAsync(roomId, year, month);
            return deleted ? NoContent() : NotFound();
        }
    }
}
