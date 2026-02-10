using Hospital.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Thêm dòng này vào đầu file

namespace Hospital.Controllers
{
    public class DichVuController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DichVuController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Trang danh sách tất cả dịch vụ
        public async Task<IActionResult> Index()
        {
            ViewBag.ChuyenKhoas = await _context.ChuyenKhoa.ToListAsync();

            // THÊM .Include(d => d.ChuyenKhoaDichVus) VÀO ĐÂY
            var services = await _context.DichVu
                .Include(d => d.ChuyenKhoaDichVus)
                .Where(s => s.IsActive)
                .OrderByDescending(s => s.IsHot)
                .ToListAsync();

            return View(services);
        }

        // Trang chi tiết một dịch vụ cụ thể
        public async Task<IActionResult> Details(int id)
        {
            var dichvu = await _context.DichVu.FindAsync(id);
            if (dichvu == null) return NotFound();

            // Lấy thêm dịch vụ liên quan để hiện ở dưới
            ViewBag.DichVuLienQuan = await _context.DichVu
                .Where(s => s.DichVuId != id && s.IsActive)
                .Take(5).ToListAsync();

            return View(dichvu);
        }
        
    }
}