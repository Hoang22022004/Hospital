using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hospital.Data;
using Hospital.Models;

namespace Hospital.Controllers
{
    public class BacsiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BacsiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================================================
        // A. TRANG DANH SÁCH (XEM TẤT CẢ BÁC SĨ)
        // ==========================================================================
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách bác sĩ kèm theo thông tin Chuyên khoa
            var doctors = await _context.BacSi
                .Include(b => b.ChuyenKhoa)
                .Where(b => b.IsActive)
                .ToListAsync();
            return View(doctors);
        }

        // ==========================================================================
        // B. TRANG CHI TIẾT BÁC SĨ
        // ==========================================================================
        public async Task<IActionResult> Details(int id)
        {
            var doctor = await _context.BacSi
                .Include(b => b.ChuyenKhoa)
                .FirstOrDefaultAsync(m => m.BacSiId == id);

            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }
    }
}