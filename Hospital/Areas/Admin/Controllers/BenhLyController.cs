using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hospital.Models;
using Hospital.Data;
using Microsoft.AspNetCore.Authorization;

namespace Hospital.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Receptionist")]
    public class BenhLyController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public BenhLyController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // 1. Danh sách bệnh lý
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 10; // Số dòng hiển thị trên mỗi trang
            var query = _context.BenhLy.OrderByDescending(x => x.NgayCapNhat);

            // 1. Tính tổng số bản ghi
            int totalItems = await query.CountAsync();

            // 2. Tính tổng số trang
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // 3. Lấy dữ liệu của trang hiện tại
            var list = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 4. Gửi dữ liệu phân trang sang View qua ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(list);
        }

        // 2. Giao diện thêm mới
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BenhLy benhLy, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem mã bệnh đã tồn tại chưa (Vì mã ICD-10 là khóa chính)
                bool exists = await _context.BenhLy.AnyAsync(x => x.BenhLyId == benhLy.BenhLyId);
                if (exists)
                {
                    TempData["error"] = "Mã bệnh lý " + benhLy.BenhLyId + " đã tồn tại trên hệ thống!";
                    return View(benhLy);
                }

                if (imageFile != null) benhLy.HinhAnhUrl = await SaveImage(imageFile);

                _context.Add(benhLy);
                await _context.SaveChangesAsync();

                // Thông báo thành công
                TempData["success"] = "Đã thêm mới bệnh lý " + benhLy.TenBenhLy + " vào danh mục!";
                return RedirectToAction(nameof(Index));
            }

            TempData["error"] = "Có lỗi xảy ra, vui lòng kiểm tra lại thông tin nhập liệu.";
            return View(benhLy);
        }

        // 3. Giao diện chỉnh sửa
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var benhLy = await _context.BenhLy.FindAsync(id);
            if (benhLy == null) return NotFound();
            return View(benhLy);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, BenhLy benhLy, IFormFile? imageFile)
        {
            if (id != benhLy.BenhLyId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null)
                    {
                        DeleteOldImage(benhLy.HinhAnhUrl);
                        benhLy.HinhAnhUrl = await SaveImage(imageFile);
                    }
                    _context.Update(benhLy);
                    await _context.SaveChangesAsync();

                    // Thông báo thành công
                    TempData["success"] = "Đã cập nhật thông tin bệnh lý " + benhLy.TenBenhLy + " thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.BenhLy.Any(e => e.BenhLyId == id)) return NotFound();
                    else
                    {
                        TempData["error"] = "Lỗi xung đột dữ liệu. Có vẻ thông tin này đã được thay đổi bởi người khác.";
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            TempData["error"] = "Không thể cập nhật thông tin. Vui lòng kiểm tra lại.";
            return View(benhLy);
        }

        // 4. Xác nhận xóa
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var benhLy = await _context.BenhLy.FirstOrDefaultAsync(m => m.BenhLyId == id);
            if (benhLy == null) return NotFound();
            return View(benhLy);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var benhLy = await _context.BenhLy.FindAsync(id);
            if (benhLy != null)
            {
                DeleteOldImage(benhLy.HinhAnhUrl);
                _context.BenhLy.Remove(benhLy);
                await _context.SaveChangesAsync();

                // Thông báo thành công
                TempData["success"] = "Đã xóa bệnh lý khỏi danh mục thành công!";
            }
            else
            {
                TempData["error"] = "Không tìm thấy bệnh lý cần xóa.";
            }

            return RedirectToAction(nameof(Index));
        }

        // --- CÁC HÀM HỖ TRỢ XỬ LÝ ẢNH ---

        private async Task<string> SaveImage(IFormFile imageFile)
        {
            string folderPath = Path.Combine(_hostEnvironment.WebRootPath, "images", "benhly");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            string fullPath = Path.Combine(folderPath, fileName);

            using (var fileStream = new FileStream(fullPath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }
            return "/images/benhly/" + fileName;
        }

        private void DeleteOldImage(string? imageUrl)
        {
            if (!string.IsNullOrEmpty(imageUrl))
            {
                var oldPath = Path.Combine(_hostEnvironment.WebRootPath, imageUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
            }
        }

        // Hỗ trợ upload ảnh từ trình soạn thảo TinyMCE
        [HttpPost]
        public async Task<IActionResult> UploadImageTiny(IFormFile file)
        {
            var path = await SaveImage(file);
            return Json(new { location = path });
        }
    }
}