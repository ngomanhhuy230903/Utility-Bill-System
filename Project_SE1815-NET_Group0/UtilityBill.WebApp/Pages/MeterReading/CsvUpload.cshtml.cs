using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UtilityBill.Business.DTOs;
using UtilityBill.Business.Interfaces;
using CsvHelper;


namespace UtilityBill.WebApp.Pages.MeterReading
{
    public class CsvUploadModel : PageModel
    {
        private readonly IMeterReadingService _service;
        public string? Message { get; private set; }

        public CsvUploadModel(IMeterReadingService service)
            => _service = service;

        public void OnGet() { }

        public async Task OnPostUploadAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                Message = "Please upload a CSV.";
                return;
            }

            List<MeterReadingUploadDto> records;
            try
            {
                using var reader = new StreamReader(file.OpenReadStream());
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                records = csv.GetRecords<MeterReadingUploadDto>().ToList();
            }
            catch
            {
                Message = "CSV format invalid.";
                return;
            }

            var (succ, fail) = await _service.BulkCreateMeterReadingsAsync(records);
            Message = $"Tổng {records.Count}: Thành công {succ}, Thất bại {fail}.";
        }
    }
}
