using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtilityBill.WebApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class NotificationsModel : PageModel
    {
        public void OnGet()
        {
        }
    }
} 