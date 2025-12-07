using Hospital.Data;
using Hospital.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using System.IO;

// Đặt Controller trong Admin Area
namespace Hospital.Areas.Admin.Controllers
{
    [Area("Admin")]
    // Chỉ cho phép người dùng có Role "Admin" truy cập
    [Authorize(Roles = "Admin")]
    public class DichVuController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment; // Khai báo biến môi trường

        // SỬA LỖI: Thêm DI cho IWebHostEnvironment
        public DichVuController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        // Action: HIỂN THỊ DANH SÁCH (READ)
        public async Task<IActionResult> Index()
        {
            var danhSachDichVu = await _db.DichVu.ToListAsync();
            return View(danhSachDichVu);
        }

        // Action: TẠO MỚI - GET
        public IActionResult Create()
        {
            return View();
        }

        // Action: TẠO MỚI - POST (Đã thêm logic xử lý file)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DichVu dichVu, IFormFile? file) // Nhận IFormFile
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;

                if (file != null)
                {
                    // Logic Lưu File
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"images\dichvu");
                    var extension = Path.GetExtension(file.FileName);

                    // Tạo thư mục nếu chưa tồn tại
                    if (!Directory.Exists(uploads))
                    {
                        Directory.CreateDirectory(uploads);
                    }

                    // Lưu file ảnh vào thư mục wwwroot
                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        await file.CopyToAsync(fileStreams);
                    }

                    // Cập nhật URL ảnh vào Model
                    dichVu.AnhDichVuUrl = @"\images\dichvu\" + fileName + extension;
                }

                _db.DichVu.Add(dichVu);
                await _db.SaveChangesAsync();
                TempData["success"] = "Thêm dịch vụ thành công.";
                return RedirectToAction(nameof(Index));
            }
            return View(dichVu);
        }

        // Action: SỬA (EDIT) - GET
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var dichVuFromDb = await _db.DichVu.FindAsync(id);

            if (dichVuFromDb == null)
            {
                return NotFound();
            }
            return View(dichVuFromDb);
        }

        // Action: SỬA (EDIT) - POST (Đã thêm logic xử lý file)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DichVu dichVu, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;

                if (file != null)
                {
                    // 1. Xóa ảnh cũ (nếu tồn tại)
                    if (dichVu.AnhDichVuUrl != null)
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, dichVu.AnhDichVuUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // 2. Lưu ảnh mới và cập nhật URL
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"images\dichvu");
                    var extension = Path.GetExtension(file.FileName);

                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        await file.CopyToAsync(fileStreams);
                    }
                    dichVu.AnhDichVuUrl = @"\images\dichvu\" + fileName + extension;
                }

                _db.DichVu.Update(dichVu);
                await _db.SaveChangesAsync();
                TempData["success"] = "Cập nhật dịch vụ thành công.";
                return RedirectToAction(nameof(Index));
            }
            return View(dichVu);
        }

        // Action: XÓA (DELETE) - GET (Hiển thị xác nhận Xóa)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var dichVuFromDb = await _db.DichVu.FindAsync(id);

            if (dichVuFromDb == null)
            {
                return NotFound();
            }
            return View(dichVuFromDb);
        }

        // Action: XÓA (DELETE) - POST (Đã thêm logic xử lý file)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePOST(int? id)
        {
            var obj = await _db.DichVu.FindAsync(id);

            if (obj == null)
            {
                return NotFound();
            }

            // LOGIC XÓA ẢNH KHỎI SERVER
            if (obj.AnhDichVuUrl != null)
            {
                var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, obj.AnhDichVuUrl.TrimStart('\\'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            _db.DichVu.Remove(obj);
            await _db.SaveChangesAsync();
            TempData["success"] = "Xóa dịch vụ thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}