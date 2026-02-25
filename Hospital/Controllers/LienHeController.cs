using Microsoft.AspNetCore.Mvc;

namespace Hospital.Controllers
{
    public class LienHeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // Hàm nhận dữ liệu khi bấm nút Gửi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitForm(string HoTen, string SoDienThoai, string Email, string NoiDung)
        {
            // (Khóa luận: Sau này bạn sẽ viết code lưu dữ liệu vào Database ở đây)

            // SỬA Ở ĐÂY: Chuyển hướng người dùng sang trang Cảm ơn thay vì load lại trang cũ
            return RedirectToAction("CamOn");
        }

        // THÊM MỚI: Hàm hiển thị trang Cảm ơn
        [HttpGet]
        public IActionResult CamOn()
        {
            return View();
        }
    }
}