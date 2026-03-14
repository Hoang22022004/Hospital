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

        // Trang danh sách bệnh lý cho khách (ĐÃ THÊM PHÂN TRANG)
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            // 1. Cấu hình số lượng bệnh lý hiển thị trên mỗi trang
            int pageSize = 8;

            // 2. Khởi tạo truy vấn (Chỉ lấy các bài viết đã xuất bản)
            var query = _context.BenhLy.Where(b => b.IsPublished).AsQueryable();

            // 3. Xử lý tìm kiếm
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(b => b.TenBenhLy.Contains(searchString) || b.BenhLyId.Contains(searchString));
            }

            // 4. Sắp xếp danh sách theo tên bệnh lý trước khi cắt trang
            query = query.OrderBy(x => x.TenBenhLy);

            // 5. Tính toán tổng số trang
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // 6. Cắt dữ liệu cho trang hiện tại
            var pagedData = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 7. Truyền các thông số sang View để vẽ nút bấm phân trang
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentSearch = searchString;

            return View(pagedData);
        }

        // Trang chi tiết bài viết bệnh lý (GIỮ NGUYÊN)
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var benhLy = await _context.BenhLy.FirstOrDefaultAsync(m => m.BenhLyId == id && m.IsPublished);

            if (benhLy == null) return NotFound();

            return View(benhLy);
        }
    }
}