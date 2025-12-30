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
        // 1. TRANG CHỦ (Cập nhật để lấy danh sách Bác sĩ)
        // ==========================================================================
        public async Task<IActionResult> Index()
        {
            // Truy vấn lấy danh sách bác sĩ đang hoạt động
            // .Include(b => b.ChuyenKhoa) để lấy được tên Chuyên khoa từ bảng liên kết
            var doctors = await _context.BacSi
                .Include(b => b.ChuyenKhoa)
                .Where(b => b.IsActive)
                .ToListAsync();

            // Trả về View kèm theo danh sách dữ liệu bác sĩ
            return View(doctors);
        }

        // ==========================================================================
        // 2. ACTION TÌM KIẾM THÔNG MINH (Giữ nguyên code cũ của bạn)
        // ==========================================================================
        [HttpGet]
        public async Task<IActionResult> GetSearchSuggestions(string term)
        {
            if (string.IsNullOrEmpty(term)) return Json(new List<object>());
            var termLower = term.ToLower();

            // 1. Tìm Bác sĩ (HoTen)
            var doctors = await _context.BacSi
                .Where(b => b.IsActive && b.HoTen.ToLower().Contains(termLower))
                .Select(b => new { title = b.HoTen, desc = b.ChuyenMon, url = "/BacSi/Details/" + b.BacSiId, img = b.HinhAnhUrl, type = "doctor" })
                .Take(3).ToListAsync();

            // 2. Tìm Dịch vụ (TenDichVu)
            var services = await _context.DichVu
                .Where(s => s.IsActive && s.TenDichVu.ToLower().Contains(termLower))
                .Select(s => new { title = s.TenDichVu, desc = s.MoTaNgan, url = "/DichVu/Details/" + s.DichVuId, img = s.AnhDichVuUrl, type = "service" })
                .Take(3).ToListAsync();

            // 3. Tìm Bệnh lý (TenBenhLy)
            var diseases = await _context.BenhLy
                .Where(b => b.TenBenhLy.ToLower().Contains(termLower))
                .Select(b => new { title = b.TenBenhLy, desc = "Thông tin bệnh lý", url = "/BenhLy/Details/" + b.BenhLyId, img = b.HinhAnhUrl, type = "disease" })
                .Take(3).ToListAsync();

            // 4. Tìm Tin tức (TieuDe)
            var news = await _context.TinTuc
                .Where(t => t.IsPublished && (t.TieuDe.ToLower().Contains(termLower) || (t.DanhMuc != null && t.DanhMuc.ToLower().Contains(termLower))))
                .Select(t => new {
                    title = t.TieuDe,
                    desc = t.DanhMuc ?? "Tin tức & Kiến thức",
                    url = "/TinTuc/Details/" + t.Id,
                    img = t.HinhAnhUrl,
                    type = "news"
                })
                .Take(3).ToListAsync();

            var results = doctors.Cast<object>()
                .Concat(services).Concat(diseases).Concat(news)
                .ToList();

            return Json(results);
        }

        // Các Action mặc định khác (Giữ nguyên)
        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}