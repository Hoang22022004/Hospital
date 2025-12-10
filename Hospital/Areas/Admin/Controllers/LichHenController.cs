using Microsoft.AspNetCore.Mvc;

namespace Hospital.Areas.Admin.Controllers
{
    public class LichHenController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
