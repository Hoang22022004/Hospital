using Hospital.Data;
using Hospital.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Receptionist,Doctor")] // Cả 3 quyền đều được vào Xem danh sách
    public class ThuocController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ThuocController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        // --- 1. DANH SÁCH THUỐC (Tất cả vào được) ---
        public async Task<IActionResult> Index()
        {
            var objList = await _db.Thuoc.ToListAsync();

            // Lấy danh sách thương hiệu để làm bộ lọc ở View
            ViewBag.ThuongHieuList = objList
                .Where(x => !string.IsNullOrEmpty(x.ThuongHieu))
                .Select(x => x.ThuongHieu)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            return View(objList);
        }

        // --- 2. TẠO MỚI THUỐC (Chỉ Admin và Receptionist) ---
        [Authorize(Roles = "Admin,Receptionist")]
        public IActionResult Create()
        {
            PrepareDonViTinhList();
            PrepareSuggestBrands();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Create(Thuoc obj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"images\thuoc");
                    var extension = Path.GetExtension(file.FileName);

                    if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        await file.CopyToAsync(fileStreams);
                    }
                    obj.HinhAnhUrl = @"\images\thuoc\" + fileName + extension;
                }

                _db.Thuoc.Add(obj);
                await _db.SaveChangesAsync();
                TempData["success"] = "Thêm thuốc vào kho thành công!";
                return RedirectToAction(nameof(Index));
            }

            PrepareDonViTinhList(obj.DonViTinh);
            PrepareSuggestBrands();
            return View(obj);
        }

        // --- 3. CHỈNH SỬA THUỐC (Chỉ Admin và Receptionist) ---
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id == 0) return NotFound();
            var obj = await _db.Thuoc.FindAsync(id);
            if (obj == null) return NotFound();

            PrepareDonViTinhList(obj.DonViTinh);
            PrepareSuggestBrands();
            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Edit(Thuoc obj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    // Xóa ảnh cũ nếu có
                    if (!string.IsNullOrEmpty(obj.HinhAnhUrl))
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, obj.HinhAnhUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath)) System.IO.File.Delete(oldImagePath);
                    }

                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"images\thuoc");
                    var extension = Path.GetExtension(file.FileName);

                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        await file.CopyToAsync(fileStreams);
                    }
                    obj.HinhAnhUrl = @"\images\thuoc\" + fileName + extension;
                }

                _db.Thuoc.Update(obj);
                await _db.SaveChangesAsync();
                TempData["success"] = "Cập nhật thông tin thuốc thành công!";
                return RedirectToAction(nameof(Index));
            }

            PrepareDonViTinhList(obj.DonViTinh);
            PrepareSuggestBrands();
            return View(obj);
        }

        // --- 4. XÓA THUỐC (Chỉ Admin và Receptionist) ---
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id == 0) return NotFound();
            var obj = await _db.Thuoc.FindAsync(id);
            if (obj == null) return NotFound();
            return View(obj);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> DeletePOST(int? id)
        {
            var obj = await _db.Thuoc.FindAsync(id);
            if (obj == null) return NotFound();

            // Xóa file ảnh trên host trước khi xóa record trong DB
            if (!string.IsNullOrEmpty(obj.HinhAnhUrl))
            {
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, obj.HinhAnhUrl.TrimStart('\\'));
                if (System.IO.File.Exists(imagePath)) System.IO.File.Delete(imagePath);
            }

            _db.Thuoc.Remove(obj);
            await _db.SaveChangesAsync();
            TempData["success"] = "Đã xóa thuốc khỏi hệ thống.";
            return RedirectToAction(nameof(Index));
        }

        // --- HÀM TRỢ GIÚP (HELPERS) ---
        private void PrepareDonViTinhList(string selectedValue = null)
        {
            List<string> donViTinhs = new List<string>() { "Hộp", "Viên", "Vỉ", "Chai", "Tuýp", "Lọ", "Gói", "Cái", "Bộ", "Liều" };
            ViewBag.DonViTinhList = new SelectList(donViTinhs, selectedValue);
        }

        private void PrepareSuggestBrands()
        {
            ViewBag.SuggestBrands = _db.Thuoc
                .Select(x => x.ThuongHieu)
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct()
                .OrderBy(x => x)
                .ToList();
        }
    }
}