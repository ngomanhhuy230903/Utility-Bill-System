using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using UtilityBill.Business.DTOs;

namespace UtilityBill.WebApp.Pages.MeterReading
{
    public class HistoryModel : PageModel
    {
        private readonly HttpClient _http;
        private readonly string _apiBase;

        public HistoryModel(IHttpClientFactory httpFactory, IConfiguration config)
        {
            _http = httpFactory.CreateClient("ApiClient");
            _apiBase = config["ApiBaseUrl"]!.TrimEnd('/');
        }

        // Filters from the GET string
        [BindProperty(SupportsGet = true)]
        public int? SelectedMonth { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SelectedYear { get; set; }

        // Bind the page number from ?pageIndex=
        [BindProperty(SupportsGet = true)]
        public int PageIndex { get; set; } = 1;

        public int PageSize { get; } = 7;
        public int TotalCount { get; private set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

        public List<MeterReadingReadDto> Items { get; private set; } = new();

        public List<SelectListItem> MonthList { get; } =
            Enumerable.Range(1, 12)
                      .Select(m => new SelectListItem(m.ToString(), m.ToString()))
                      .ToList();

        public List<SelectListItem> YearList { get; } =
            Enumerable.Range(DateTime.Now.Year - 2, 5)
                      .Select(y => new SelectListItem(y.ToString(), y.ToString()))
                      .ToList();

        public async Task OnGetAsync()
        {
            // Build the API URL with exactly the same query string
            var url = $"{_apiBase}/meterreading/history"
                    + $"?page={PageIndex}"
                    + $"&pageSize={PageSize}"
                    + (SelectedMonth.HasValue ? $"&month={SelectedMonth}" : "")
                    + (SelectedYear.HasValue ? $"&year={SelectedYear}" : "");

            var resp = await _http.GetFromJsonAsync<HistoryResponseDto>(url);
            if (resp is not null)
            {
                Items = resp.Items;
                TotalCount = resp.TotalCount;
            }
        }
    }

    public class HistoryResponseDto
    {
        public List<MeterReadingReadDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
    }
}