using Microsoft.AspNetCore.Mvc;

namespace Hospital.Models
{
    public class PaymentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
