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
        // 1. TRANG CHỦ: Lấy dữ liệu Bác sĩ, Dịch vụ và Tin tức
        // ==========================================================================
        public async Task<IActionResult> Index()
        {
            // Bước A: Lấy danh sách bác sĩ đang hoạt động làm Model chính
            var doctors = await _context.BacSi
                .Include(b => b.ChuyenKhoa)
                .Where(b => b.IsActive)
                .ToListAsync();

            // Bước B: Lấy danh sách dịch vụ đang hoạt động bỏ vào ViewBag
            // Sắp xếp ưu tiên các dịch vụ nổi bật (IsHot) lên đầu
            ViewBag.DanhSachDichVu = await _context.DichVu
                .Where(s => s.IsActive)
                .OrderByDescending(s => s.IsHot)
                .ThenByDescending(s => s.DichVuId)
                .Take(8) // Chỉ lấy 8 dịch vụ tiêu biểu cho trang chủ
                .ToListAsync();

            // Bước C: Lấy 3 tin tức mới nhất nạp vào ViewBag để hiển thị Section Tin tức
            // Phần này sẽ sửa lỗi "listTinTuc does not exist" ở ngoài View
            ViewBag.ListTinTuc = await _context.TinTuc
                .Where(t => t.IsPublished)
                .OrderByDescending(t => t.NgayDang)
                .Take(10)
                .ToListAsync();

            ViewBag.ListBenhLy = await _context.BenhLy
        .Where(b => b.IsPublished)
        .OrderBy(b => b.TenBenhLy)
        .Take(8)
        .ToListAsync();
            // Trả về View Index kèm danh sách bác sĩ
            return View(doctors);
        }

        // ==========================================================================
        // 2. ACTION TÌM KIẾM THÔNG MINH (AJAX)
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

            // Hợp nhất kết quả
            var results = doctors.Cast<object>()
                .Concat(services.Cast<object>())
                .Concat(diseases.Cast<object>())
                .Concat(news.Cast<object>())
                .ToList();

            return Json(results);
        }

        // ==========================================================================
        // 3. CÁC ACTION MẶC ĐỊNH KHÁC
        // ==========================================================================
        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}