using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hospital.Data;
using Hospital.Models;

namespace Hospital.Controllers
{
    public class TinTucController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TinTucController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Trang danh sách bài viết có bộ lọc chuyên mục
        public async Task<IActionResult> Index(string category)
        {
            var query = _context.TinTuc.Where(t => t.IsPublished);

            // Logic lọc theo danh mục
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(t => t.DanhMuc == category);
                ViewBag.CurrentCategory = category;
            }

            var list = await query.OrderByDescending(t => t.NgayDang).ToListAsync();
            return View(list);
        }

        // Trang chi tiết bài viết cho khách xem
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var tinTuc = await _context.TinTuc.FirstOrDefaultAsync(m => m.Id == id);
            if (tinTuc == null) return NotFound();

            // Lấy 3 bài viết liên quan (cùng danh mục)
            ViewBag.RelatedPosts = await _context.TinTuc
                .Where(x => x.DanhMuc == tinTuc.DanhMuc && x.Id != tinTuc.Id && x.IsPublished)
                .Take(3).ToListAsync();

            // Lấy 5 bài viết mới nhất cho Sidebar
            ViewBag.RecentPosts = await _context.TinTuc
                .Where(x => x.IsPublished && x.Id != tinTuc.Id)
                .OrderByDescending(x => x.NgayDang).Take(5).ToListAsync();

            return View(tinTuc);
        }
    }
}