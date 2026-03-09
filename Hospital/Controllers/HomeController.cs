using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hospital.Data;
using Hospital.Models;

namespace Hospital.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ==========================================================================
        // 1. TRANG CHỦ: Model chính là Danh sách Bác sĩ để chạy Carousel
        // ==========================================================================
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách bác sĩ đang hoạt động làm Model chính
            var doctors = await _context.BacSi
                .Include(b => b.ChuyenKhoa)
                .Where(b => b.IsActive)
                .ToListAsync();

            // Lấy 8 dịch vụ tiêu biểu bỏ vào ViewBag
            ViewBag.DanhSachDichVu = await _context.DichVu
                .Where(s => s.IsActive)
                .OrderByDescending(s => s.IsHot)
                .Take(8)
                .ToListAsync();

            // Lấy 10 tin tức mới nhất
            ViewBag.ListTinTuc = await _context.TinTuc
                .Where(t => t.IsPublished)
                .OrderByDescending(t => t.NgayDang)
                .Take(10)
                .ToListAsync();

            // Lấy 8 bệnh lý tiêu biểu
            ViewBag.ListBenhLy = await _context.BenhLy
                .Where(b => b.IsPublished)
                .OrderBy(b => b.TenBenhLy)
                .Take(8)
                .ToListAsync();

            return View(doctors);
        }

        // ==========================================================================
        // 2. ACTION TÌM KIẾM: Sửa URL trỏ về Controller Bacsi
        // ==========================================================================
        [HttpGet]
        public async Task<IActionResult> GetSearchSuggestions(string term)
        {
            if (string.IsNullOrEmpty(term)) return Json(new List<object>());
            var termLower = term.ToLower();

            // Sửa đường dẫn URL từ /BacSi/ sang /Bacsi/ cho đồng bộ
            var doctors = await _context.BacSi
                .Where(b => b.IsActive && b.HoTen.ToLower().Contains(termLower))
                .Select(b => new {
                    title = b.HoTen,
                    desc = b.ChuyenMon,
                    url = "/Bacsi/Details/" + b.BacSiId, // ĐÃ SỬA ĐƯỜNG DẪN
                    img = b.HinhAnhUrl,
                    type = "doctor"
                })
                .Take(3).ToListAsync();

            // (Các phần tìm kiếm Dịch vụ, Bệnh lý, Tin tức giữ nguyên như cũ)
            var services = await _context.DichVu
                .Where(s => s.IsActive && s.TenDichVu.ToLower().Contains(termLower))
                .Select(s => new { title = s.TenDichVu, desc = s.MoTaNgan, url = "/DichVu/Details/" + s.DichVuId, img = s.AnhDichVuUrl, type = "service" })
                .Take(3).ToListAsync();

            var diseases = await _context.BenhLy
                .Where(b => b.TenBenhLy.ToLower().Contains(termLower))
                .Select(b => new { title = b.TenBenhLy, desc = "Thông tin bệnh lý", url = "/BenhLy/Details/" + b.BenhLyId, img = b.HinhAnhUrl, type = "disease" })
                .Take(3).ToListAsync();

            var news = await _context.TinTuc
                .Where(t => t.IsPublished && t.TieuDe.ToLower().Contains(termLower))
                .Select(t => new { title = t.TieuDe, desc = "Tin tức & Kiến thức", url = "/TinTuc/Details/" + t.Id, img = t.HinhAnhUrl, type = "news" })
                .Take(3).ToListAsync();

            var results = doctors.Cast<object>().Concat(services).Concat(diseases).Concat(news).ToList();
            return Json(results);
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}