using Microsoft.AspNetCore.Mvc;

namespace UtilityBill.Api.Controllers
{
    public class MeterReadingController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
