using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hospital.Data;

namespace Hospital.Controllers
{
    public class BenhLyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BenhLyController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Trang danh sách bệnh lý cho khách
        public async Task<IActionResult> Index(string searchString)
        {
            var query = _context.BenhLy.Where(b => b.IsPublished).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(b => b.TenBenhLy.Contains(searchString) || b.BenhLyId.Contains(searchString));
            }

            return View(await query.OrderBy(x => x.TenBenhLy).ToListAsync());
        }

        // Trang chi tiết bài viết bệnh lý
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var benhLy = await _context.BenhLy.FirstOrDefaultAsync(m => m.BenhLyId == id && m.IsPublished);

            if (benhLy == null) return NotFound();

            return View(benhLy);
        }
    }
}