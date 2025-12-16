using Hospital.Data;
using Hospital.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Chỉ Admin mới được nhập kho/sửa thuốc
    public class ThuocController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ThuocController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        // --- HÀM PHỤ TRỢ: TẠO DANH SÁCH ĐƠN VỊ TÍNH (HARDCODE) ---
        private void PrepareDonViTinhList(string selectedValue = null)
        {
            List<string> donViTinhs = new List<string>()
            {
                "Hộp", "Viên", "Vỉ", "Chai", "Tuýp", "Lọ", "Gói", "Cái", "Bộ", "Liều"
            };
            ViewBag.DonViTinhList = new SelectList(donViTinhs, selectedValue);
        }

        // --- HÀM PHỤ TRỢ: LẤY DANH SÁCH GỢI Ý THƯƠNG HIỆU ---
        private void PrepareSuggestBrands()
        {
            ViewBag.SuggestBrands = _db.Thuoc
                .Select(x => x.ThuongHieu)
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct()
                .OrderBy(x => x)
                .ToList();
        }

        // 1. DANH SÁCH (INDEX)
        public async Task<IActionResult> Index()
        {
            var objList = await _db.Thuoc.ToListAsync();

            // Lấy danh sách thương hiệu ĐỘC NHẤT (Distinct) để làm bộ lọc
            ViewBag.ThuongHieuList = objList
                .Where(x => !string.IsNullOrEmpty(x.ThuongHieu))
                .Select(x => x.ThuongHieu)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            return View(objList);
        }

        // 2. TẠO MỚI (GET)
        public IActionResult Create()
        {
            PrepareDonViTinhList(); // Nạp danh sách đơn vị
            PrepareSuggestBrands(); // Nạp danh sách gợi ý thương hiệu
            return View();
        }

        // 3. TẠO MỚI (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
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
                TempData["success"] = "Thêm thuốc mới thành công!";
                return RedirectToAction(nameof(Index));
            }

            // Nếu lỗi, nạp lại các danh sách
            PrepareDonViTinhList(obj.DonViTinh);
            PrepareSuggestBrands();
            return View(obj);
        }

        // 4. CHỈNH SỬA (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id == 0) return NotFound();
            var obj = await _db.Thuoc.FindAsync(id);
            if (obj == null) return NotFound();

            PrepareDonViTinhList(obj.DonViTinh); // Nạp danh sách đơn vị
            PrepareSuggestBrands(); // Nạp danh sách gợi ý thương hiệu

            return View(obj);
        }

        // 5. CHỈNH SỬA (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Thuoc obj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
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
                TempData["success"] = "Cập nhật thuốc thành công!";
                return RedirectToAction(nameof(Index));
            }

            PrepareDonViTinhList(obj.DonViTinh);
            PrepareSuggestBrands();
            return View(obj);
        }

        // 6. XÓA (DELETE - GET)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id == 0) return NotFound();
            var obj = await _db.Thuoc.FindAsync(id);
            if (obj == null) return NotFound();
            return View(obj);
        }

        // 7. XÓA (DELETE - POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePOST(int? id)
        {
            var obj = await _db.Thuoc.FindAsync(id);
            if (obj == null) return NotFound();

            if (!string.IsNullOrEmpty(obj.HinhAnhUrl))
            {
                var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, obj.HinhAnhUrl.TrimStart('\\'));
                if (System.IO.File.Exists(oldImagePath)) System.IO.File.Delete(oldImagePath);
            }

            _db.Thuoc.Remove(obj);
            await _db.SaveChangesAsync();
            TempData["success"] = "Đã xóa thuốc khỏi kho.";
            return RedirectToAction(nameof(Index));
        }
    }
}