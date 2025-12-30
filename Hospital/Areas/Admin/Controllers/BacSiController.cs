using Hospital.Data;
using Hospital.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hospital.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BacSiController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;

        public BacSiController(ApplicationDbContext db,
                               IWebHostEnvironment webHostEnvironment,
                               UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        // Action phụ trợ: Lấy danh sách Chuyên khoa
        private void PrepareChuyenKhoaData(object selectedChuyenKhoa = null)
        {
            var chuyenKhoaList = _db.ChuyenKhoa
                                    .OrderBy(c => c.TenChuyenKhoa)
                                    .Select(c => new { c.ChuyenKhoaId, c.TenChuyenKhoa })
                                    .ToList();

            ViewData["ChuyenKhoaId"] = new SelectList(chuyenKhoaList, "ChuyenKhoaId", "TenChuyenKhoa", selectedChuyenKhoa);
        }

        // Action phụ trợ: Lấy danh sách User (Role = Doctor)
        private async Task PrepareUserData(string selectedUserId = null)
        {
            var doctorUsers = await _userManager.GetUsersInRoleAsync("Doctor");

            var assignedUserIds = await _db.BacSi
                .Where(b => b.IdentityUserId != selectedUserId)
                .Select(b => b.IdentityUserId)
                .ToListAsync();

            var availableUsers = doctorUsers
                .Where(u => !assignedUserIds.Contains(u.Id))
                .Select(u => new
                {
                    Id = u.Id,
                    DisplayText = $"{u.FullName ?? u.UserName} ({u.Email})"
                })
                .ToList();

            ViewData["IdentityUserId"] = new SelectList(availableUsers, "Id", "DisplayText", selectedUserId);
        }

        // Action: HIỂN THỊ DANH SÁCH (READ)
        public async Task<IActionResult> Index()
        {
            var danhSachBacSi = await _db.BacSi
                .Include(b => b.User)
                .Include(b => b.ChuyenKhoa)
                .ToListAsync();
            return View(danhSachBacSi);
        }

        // Action: TẠO MỚI - GET
        public async Task<IActionResult> Create()
        {
            await PrepareUserData();
            PrepareChuyenKhoaData();
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
                    var uploads = Path.Combine(wwwRootPath, @"images\bacsi");
                    var extension = Path.GetExtension(file.FileName);

                    if (!Directory.Exists(uploads))
                    {
                        Directory.CreateDirectory(uploads);
                    }

                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        await file.CopyToAsync(fileStreams);
                    }

                    bacSi.HinhAnhUrl = @"\images\bacsi\" + fileName + extension;
                }

                _db.BacSi.Add(bacSi);
                await _db.SaveChangesAsync();
                TempData["success"] = "Thêm bác sĩ thành công.";
                return RedirectToAction(nameof(Index));
            }

            await PrepareUserData(bacSi.IdentityUserId);
            PrepareChuyenKhoaData(bacSi.ChuyenKhoaId);
            return View(bacSi);
        }

        // Action: SỬA (EDIT) - GET
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id == 0) return NotFound();

            var bacSiFromDb = await _db.BacSi.FindAsync(id);
            if (bacSiFromDb == null) return NotFound();

            await PrepareUserData(bacSiFromDb.IdentityUserId);
            PrepareChuyenKhoaData(bacSiFromDb.ChuyenKhoaId);

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
                    if (!string.IsNullOrEmpty(bacSi.HinhAnhUrl))
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, bacSi.HinhAnhUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

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

            await PrepareUserData(bacSi.IdentityUserId);
            PrepareChuyenKhoaData(bacSi.ChuyenKhoaId);
            return View(bacSi);
        }

        // Action: XÓA (DELETE) - GET
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id == 0) return NotFound();

            var bacSiFromDb = await _db.BacSi
                                       .Include(b => b.User)
                                       .Include(b => b.ChuyenKhoa)
                                       .FirstOrDefaultAsync(b => b.BacSiId == id);

            if (bacSiFromDb == null) return NotFound();

            return View(bacSiFromDb);
        }

        // Action: XÓA (DELETE) - POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePOST(int? id)
        {
            var obj = await _db.BacSi.FindAsync(id);
            if (obj == null) return NotFound();

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

        // --- ACTION MỚI: HỖ TRỢ UPLOAD ẢNH TỪ TRÌNH SOẠN THẢO TINYMCE ---
        [HttpPost]
        public async Task<IActionResult> UploadImageTiny(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest();

            string wwwRootPath = _webHostEnvironment.WebRootPath;
            string fileName = Guid.NewGuid().ToString();
            var uploads = Path.Combine(wwwRootPath, @"images\bacsi");
            var extension = Path.GetExtension(file.FileName);

            if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

            using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
            {
                await file.CopyToAsync(fileStreams);
            }

            // Trả về URL để TinyMCE hiển thị ảnh trong trình soạn thảo
            return Json(new { location = "/images/bacsi/" + fileName + extension });
        }
    }
}