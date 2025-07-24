using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace UtilityBill.WebApp.Pages
{
    public class MyHomepageModel : PageModel
    {
        private readonly ILogger<MyHomepageModel> _logger;

        public MyHomepageModel(ILogger<MyHomepageModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}
