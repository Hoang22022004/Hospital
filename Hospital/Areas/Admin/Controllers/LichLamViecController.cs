using Hospital.Data;
using Hospital.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Hospital.Areas.Admin.Controllers
{
    [Area("Admin")]
    // CẬP NHẬT: Cho phép Admin, Doctor và Receptionist truy cập
    [Authorize(Roles = "Admin,Doctor,Receptionist")]
    public class LichLamViecController : Controller
    {
        private readonly ApplicationDbContext _db;

        public LichLamViecController(ApplicationDbContext db)
        {
            _db = db;
        }

        // --- HÀM HELPER: Lấy thông tin Bác sĩ từ Identity UserId ---
        private async Task<BacSi?> GetCurrentBacSi()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return await _db.BacSi.FirstOrDefaultAsync(u => u.IdentityUserId == userId);
        }

        private void PrepareBacSiSelectList(object selectedBacSi = null)
        {
            // CẬP NHẬT: Admin và Receptionist được quyền xem tất cả bác sĩ
            if (User.IsInRole("Admin") || User.IsInRole("Receptionist"))
            {
                ViewData["BacSiId"] = new SelectList(_db.BacSi, "BacSiId", "HoTen", selectedBacSi);
                ViewBag.BacSis = new SelectList(_db.BacSi.OrderBy(b => b.HoTen), "HoTen", "HoTen");
            }
            // Nếu là Doctor thì danh sách chỉ chứa chính họ
            else if (User.IsInRole("Doctor"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var currentDoc = _db.BacSi.Where(u => u.IdentityUserId == userId).ToList();
                ViewData["BacSiId"] = new SelectList(currentDoc, "BacSiId", "HoTen", selectedBacSi);
            }
        }

        // --- 1. DANH SÁCH LỊCH LÀM VIỆC (INDEX) ---
        public async Task<IActionResult> Index()
        {
            PrepareBacSiSelectList();
            var query = _db.LichLamViec.Include(l => l.BacSi).AsQueryable();

            // LOGIC: Chỉ Bác sĩ mới bị lọc dữ liệu. Admin và Receptionist thấy toàn bộ.
            if (User.IsInRole("Doctor"))
            {
                var bacSi = await GetCurrentBacSi();
                if (bacSi != null)
                {
                    query = query.Where(l => l.BacSiId == bacSi.BacSiId);
                }
            }

            return View(await query.ToListAsync());
        }

        // --- 2. THÊM MỚI CA LÀM VIỆC (CREATE) ---
        public async Task<IActionResult> Create()
        {
            PrepareBacSiSelectList();

            // Nếu là Doctor, chuẩn bị dữ liệu khóa cứng tên bác sĩ
            if (User.IsInRole("Doctor"))
            {
                var bacSi = await GetCurrentBacSi();
                if (bacSi != null)
                {
                    ViewBag.CurrentBacSiId = bacSi.BacSiId;
                    ViewBag.CurrentBacSiName = bacSi.HoTen;
                }
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LichLamViec lichLamViec)
        {
            // BẢO MẬT: Nếu là Doctor, ép buộc ID phải là ID của chính họ
            if (User.IsInRole("Doctor"))
            {
                var bacSi = await GetCurrentBacSi();
                if (bacSi != null) lichLamViec.BacSiId = bacSi.BacSiId;

                ModelState.Remove("BacSi");
                ModelState.Remove("BacSiId");
            }

            if (ModelState.IsValid)
            {
                bool isDuplicate = _db.LichLamViec.Any(l =>
                    l.BacSiId == lichLamViec.BacSiId &&
                    l.NgayLamViec.Date == lichLamViec.NgayLamViec.Date
                );

                if (isDuplicate)
                {
                    ModelState.AddModelError("NgayLamViec", "Bác sĩ này đã có lịch làm việc trong ngày này rồi.");
                }
                else if (lichLamViec.GioKetThuc <= lichLamViec.GioBatDau)
                {
                    ModelState.AddModelError("GioKetThuc", "Giờ kết thúc phải sau giờ bắt đầu.");
                }
                else
                {
                    _db.LichLamViec.Add(lichLamViec);
                    await _db.SaveChangesAsync();
                    TempData["success"] = "Đăng ký ca làm việc thành công!";
                    return RedirectToAction("Index");
                }
            }

            if (User.IsInRole("Doctor"))
            {
                var bacSi = await GetCurrentBacSi();
                ViewBag.CurrentBacSiId = bacSi?.BacSiId;
                ViewBag.CurrentBacSiName = bacSi?.HoTen;
            }

            PrepareBacSiSelectList(lichLamViec.BacSiId);
            return View(lichLamViec);
        }

        // --- 3. CHỈNH SỬA CA LÀM VIỆC (EDIT) ---
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id == 0) return NotFound();

            var lichLamViecFromDb = await _db.LichLamViec.FindAsync(id);
            if (lichLamViecFromDb == null) return NotFound();

            // KIỂM TRA QUYỀN: Bác sĩ không được sửa lịch người khác qua URL. Admin/Receptionist sửa thoải mái.
            if (User.IsInRole("Doctor"))
            {
                var bacSi = await GetCurrentBacSi();
                if (bacSi == null || lichLamViecFromDb.BacSiId != bacSi.BacSiId) return Forbid();
            }

            PrepareBacSiSelectList(lichLamViecFromDb.BacSiId);
            return View(lichLamViecFromDb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(LichLamViec lichLamViec)
        {
            if (User.IsInRole("Doctor"))
            {
                var bacSi = await GetCurrentBacSi();
                if (bacSi == null || lichLamViec.BacSiId != bacSi.BacSiId) return Forbid();
                ModelState.Remove("BacSi");
            }

            if (ModelState.IsValid)
            {
                _db.LichLamViec.Update(lichLamViec);
                await _db.SaveChangesAsync();
                TempData["success"] = "Cập nhật ca làm việc thành công!";
                return RedirectToAction("Index");
            }

            PrepareBacSiSelectList(lichLamViec.BacSiId);
            return View(lichLamViec);
        }

        // --- 4. XÓA CA LÀM VIỆC (DELETE) ---
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id == 0) return NotFound();
            var lichLamViecFromDb = await _db.LichLamViec.Include(l => l.BacSi).FirstOrDefaultAsync(l => l.LichLamViecId == id);

            if (lichLamViecFromDb == null) return NotFound();

            if (User.IsInRole("Doctor"))
            {
                var bacSi = await GetCurrentBacSi();
                if (bacSi == null || lichLamViecFromDb.BacSiId != bacSi.BacSiId) return Forbid();
            }

            return View(lichLamViecFromDb);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePOST(int? id)
        {
            var obj = await _db.LichLamViec.FindAsync(id);
            if (obj == null) return NotFound();

            if (User.IsInRole("Doctor"))
            {
                var bacSi = await GetCurrentBacSi();
                if (bacSi == null || obj.BacSiId != bacSi.BacSiId) return Forbid();
            }

            bool hasAppointments = await _db.LichHen.AnyAsync(lh => lh.LichLamViecId == id);
            if (hasAppointments)
            {
                TempData["error"] = "Không thể xóa ca làm việc đã có khách đặt lịch!";
            }
            else
            {
                _db.LichLamViec.Remove(obj);
                await _db.SaveChangesAsync();
                TempData["success"] = "Xóa ca làm việc thành công!";
            }

            return RedirectToAction("Index");
        }
    }
}