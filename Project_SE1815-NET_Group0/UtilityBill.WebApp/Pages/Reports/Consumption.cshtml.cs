using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtilityBill.WebApp.Pages.Reports
{
    [Authorize]
    public class ConsumptionModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
