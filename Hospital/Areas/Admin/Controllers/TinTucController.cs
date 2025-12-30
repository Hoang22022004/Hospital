using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hospital.Models;
using Hospital.Data;
using Microsoft.AspNetCore.Authorization;
using System.IO;

namespace Hospital.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Receptionist")]
    public class TinTucController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public TinTucController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // 1. Danh sách tin tức
        public async Task<IActionResult> Index()
        {
            var list = await _context.TinTuc.OrderByDescending(x => x.NgayDang).ToListAsync();
            return View(list);
        }

        // 2. Chi tiết bài viết
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var tinTuc = await _context.TinTuc.FirstOrDefaultAsync(m => m.Id == id);
            if (tinTuc == null) return NotFound();
            return View(tinTuc);
        }

        // 3. Giao diện thêm mới
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TinTuc tinTuc, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null)
                {
                    tinTuc.HinhAnhUrl = await SaveImage(imageFile);
                }
                _context.Add(tinTuc);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tinTuc);
        }

        // 4. Giao diện chỉnh sửa
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var tinTuc = await _context.TinTuc.FindAsync(id);
            if (tinTuc == null) return NotFound();
            return View(tinTuc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TinTuc tinTuc, IFormFile? imageFile)
        {
            if (id != tinTuc.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null)
                    {
                        // Xóa ảnh cũ nếu có ảnh mới
                        DeleteOldImage(tinTuc.HinhAnhUrl);
                        tinTuc.HinhAnhUrl = await SaveImage(imageFile);
                    }
                    _context.Update(tinTuc);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TinTucExists(tinTuc.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(tinTuc);
        }

        // 5. Xóa bài viết
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var tinTuc = await _context.TinTuc.FirstOrDefaultAsync(m => m.Id == id);
            if (tinTuc == null) return NotFound();
            return View(tinTuc);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tinTuc = await _context.TinTuc.FindAsync(id);
            if (tinTuc != null)
            {
                DeleteOldImage(tinTuc.HinhAnhUrl);
                _context.TinTuc.Remove(tinTuc);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // --- HÀM HỖ TRỢ (PRIVATE) ---

        private bool TinTucExists(int id) => _context.TinTuc.Any(e => e.Id == id);

        private async Task<string> SaveImage(IFormFile imageFile)
        {
            string folderPath = Path.Combine(_hostEnvironment.WebRootPath, "images", "tintuc");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            string fullPath = Path.Combine(folderPath, fileName);

            using (var fileStream = new FileStream(fullPath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }
            return "/images/tintuc/" + fileName;
        }

        private void DeleteOldImage(string? imageUrl)
        {
            if (!string.IsNullOrEmpty(imageUrl))
            {
                var oldPath = Path.Combine(_hostEnvironment.WebRootPath, imageUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadImageTiny(IFormFile file)
        {
            var path = await SaveImage(file);
            return Json(new { location = path });
        }
    }
}