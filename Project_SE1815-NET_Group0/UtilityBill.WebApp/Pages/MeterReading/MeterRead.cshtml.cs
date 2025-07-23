using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using UtilityBill.Business.DTOs;

namespace UtilityBill.WebApp.Pages.MeterReading
{
    public class MeterReadModel : PageModel
    {
        private readonly HttpClient _http;

        public MeterReadModel(IHttpClientFactory httpFactory)
          => _http = httpFactory.CreateClient("ApiClient");

        // for the dropdown
        public List<RoomDto> Rooms { get; set; } = new();

        // manual entry
        [BindProperty] public MeterReadingCreateDto Input { get; set; } = new();

        // CSV file
        [BindProperty] public IFormFile? CsvFile { get; set; }

        public async Task OnGetAsync()
        {
            Rooms = await _http.GetFromJsonAsync<List<RoomDto>>("rooms")
                   ?? new List<RoomDto>();

            // default today's date & user
            var today = DateTime.Today;
            Input.ReadingDate = today;
            Input.ReadingMonth = today.Month;
            Input.ReadingYear = today.Year;
            Input.RecordedByUserId = "admin-user-guid";
        }

        // Manual entry handler
        public async Task<IActionResult> OnPostCreateReadingAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            var resp = await _http.PostAsJsonAsync("meterreading/create-reading", Input);
            if (resp.IsSuccessStatusCode)
                return RedirectToPage("History", new { month = Input.ReadingMonth, year = Input.ReadingYear });

            ModelState.AddModelError("", await resp.Content.ReadAsStringAsync());
            await OnGetAsync();
            return Page();
        }

        // CSV upload handler
        public async Task<IActionResult> OnPostUploadCsvAsync()
        {
            if (CsvFile == null || CsvFile.Length == 0)
            {
                ModelState.AddModelError("CsvFile", "Vui lòng chọn file CSV.");
                await OnGetAsync();
                return Page();
            }

            var content = new MultipartFormDataContent();
            using var stream = CsvFile.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(CsvFile.ContentType);
            content.Add(fileContent, "file", CsvFile.FileName);

            var resp = await _http.PostAsync("meterreading/bulk-upload", content);

            if (!resp.IsSuccessStatusCode)
                ModelState.AddModelError("", $"Upload thất bại: {resp.ReasonPhrase}");
            else
            {
                // read back your DTO: { total, success, failed }
                var result = await resp.Content.ReadFromJsonAsync<BulkUploadResultDto>();
                TempData["UploadMessage"] =
                  $"Tổng: {result?.Total}, Thành công: {result?.Success}, Thất bại: {result?.Failed}";
            }

            await OnGetAsync();
            return Page();
        }
    }
}
