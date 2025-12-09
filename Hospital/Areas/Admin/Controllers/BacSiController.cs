using Hospital.Data;
using Hospital.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Identity; // Cần cho UserManager
using Microsoft.AspNetCore.Mvc.Rendering; // Cần cho SelectList

// Đặt Controller trong Admin Area
namespace Hospital.Areas.Admin.Controllers
{
    [Area("Admin")]
    // Chỉ cho phép người dùng có Role "Admin" truy cập
    [Authorize(Roles = "Admin")]
    public class BacSiController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        // KHAI BÁO THÊM USER MANAGER ĐỂ QUẢN LÝ TÀI KHOẢN
        private readonly UserManager<ApplicationUser> _userManager;

        // Cấu hình Dependency Injection (DI)
        public BacSiController(ApplicationDbContext db,
                               IWebHostEnvironment webHostEnvironment,
                               UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        // Action: HIỂN THỊ DANH SÁCH (READ)
        public async Task<IActionResult> Index()
        {
            // Eager loading ApplicationUser để hiển thị email/tên tài khoản
            var danhSachBacSi = await _db.BacSi
                .Include(b => b.User)
                .ToListAsync();
            return View(danhSachBacSi);
        }

        // Action: TẠO MỚI - GET
        public async Task<IActionResult> Create()
        {
            // Lấy danh sách người dùng CHƯA CÓ hồ sơ Bác sĩ để chọn tài khoản liên kết
            var usersWithoutBacSiProfile = await _db.ApplicationUser
                .Where(u => !_db.BacSi.Any(b => b.IdentityUserId == u.Id))
                .Select(u => new { u.Id, u.Email })
                .ToListAsync();

            ViewData["IdentityUserId"] = new SelectList(usersWithoutBacSiProfile, "Id", "Email");
            return View();
        }

        // Action: TẠO MỚI - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BacSi bacSi, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;

                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"images\bacsi"); // Thư mục lưu ảnh Bác sĩ
                    var extension = Path.GetExtension(file.FileName);

                    if (!Directory.Exists(uploads))
                    {
                        Directory.CreateDirectory(uploads);
                    }

                    // Lưu file ảnh
                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        await file.CopyToAsync(fileStreams);
                    }

                    // Cập nhật URL ảnh vào Model
                    bacSi.HinhAnhUrl = @"\images\bacsi\" + fileName + extension;
                }

                _db.BacSi.Add(bacSi);
                await _db.SaveChangesAsync();
                TempData["success"] = "Thêm bác sĩ thành công.";
                return RedirectToAction(nameof(Index));
            }

            // Nếu lỗi, load lại danh sách User cho View
            var usersWithoutBacSiProfile = await _db.ApplicationUser
                .Where(u => !_db.BacSi.Any(b => b.IdentityUserId == u.Id))
                .Select(u => new { u.Id, u.Email })
                .ToListAsync();
            ViewData["IdentityUserId"] = new SelectList(usersWithoutBacSiProfile, "Id", "Email", bacSi.IdentityUserId);
            return View(bacSi);
        }

        // Action: SỬA (EDIT) - GET
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var bacSiFromDb = await _db.BacSi.FindAsync(id);

            if (bacSiFromDb == null)
            {
                return NotFound();
            }

            // Lấy danh sách người dùng: User hiện tại hoặc User chưa có profile
            var usersAvailable = await _db.ApplicationUser
                .Where(u => !_db.BacSi.Any(b => b.IdentityUserId == u.Id) || u.Id == bacSiFromDb.IdentityUserId)
                .Select(u => new { u.Id, u.Email })
                .ToListAsync();

            ViewData["IdentityUserId"] = new SelectList(usersAvailable, "Id", "Email", bacSiFromDb.IdentityUserId);
            return View(bacSiFromDb);
        }

        // Action: SỬA (EDIT) - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BacSi bacSi, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;

                if (file != null)
                {
                    // 1. Xóa ảnh cũ
                    if (!string.IsNullOrEmpty(bacSi.HinhAnhUrl))
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, bacSi.HinhAnhUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // 2. Lưu ảnh mới và cập nhật URL
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"images\bacsi");
                    var extension = Path.GetExtension(file.FileName);

                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        await file.CopyToAsync(fileStreams);
                    }

                    bacSi.HinhAnhUrl = @"\images\bacsi\" + fileName + extension;
                }

                _db.BacSi.Update(bacSi);
                await _db.SaveChangesAsync();
                TempData["success"] = "Cập nhật thông tin bác sĩ thành công.";
                return RedirectToAction(nameof(Index));
            }

            // Nếu lỗi, load lại danh sách User cho View
            var usersAvailable = await _db.ApplicationUser
                .Where(u => !_db.BacSi.Any(b => b.IdentityUserId == u.Id) || u.Id == bacSi.IdentityUserId)
                .Select(u => new { u.Id, u.Email })
                .ToListAsync();

            ViewData["IdentityUserId"] = new SelectList(usersAvailable, "Id", "Email", bacSi.IdentityUserId);
            return View(bacSi);
        }

        // Action: XÓA (DELETE) - GET
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var bacSiFromDb = await _db.BacSi.Include(b => b.User).FirstOrDefaultAsync(b => b.BacSiId == id);

            if (bacSiFromDb == null)
            {
                return NotFound();
            }
            return View(bacSiFromDb);
        }

        // Action: XÓA (DELETE) - POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePOST(int? id)
        {
            var obj = await _db.BacSi.FindAsync(id);

            if (obj == null)
            {
                return NotFound();
            }

            // LOGIC XÓA ẢNH KHỎI SERVER
            if (obj.HinhAnhUrl != null)
            {
                var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, obj.HinhAnhUrl.TrimStart('\\'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            _db.BacSi.Remove(obj);
            await _db.SaveChangesAsync();
            TempData["success"] = "Xóa bác sĩ thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}