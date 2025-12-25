using Hospital.Data;
using Hospital.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using static Hospital.Models.LichHen;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Hospital.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Doctor,Receptionist")] // Cả 3 quyền đều vào được
    public class LichHenController : Controller
    {
        private readonly ApplicationDbContext _db;

        public LichHenController(ApplicationDbContext db)
        {
            _db = db;
        }

        // --- HÀM HELPER: Lấy thông tin Bác sĩ từ tài khoản đang đăng nhập ---
        private async Task<BacSi?> GetCurrentBacSi()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return await _db.BacSi.FirstOrDefaultAsync(u => u.IdentityUserId == userId);
        }

        // 1. DANH SÁCH LỊCH HẸN (INDEX)
        public async Task<IActionResult> Index()
        {
            PrepareViewData();
            var query = _db.LichHen.Include(l => l.BacSi).Include(l => l.DichVu).Include(l => l.LichLamViec).AsQueryable();

            // LOGIC: Nếu là Doctor, chỉ lọc ra lịch của chính họ. Admin và Receptionist thấy hết.
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

        // 2. DUYỆT LỊCH HẸN
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Receptionist,Doctor")] // Cho phép cả 3, nhưng bảo mật riêng cho Doctor
        public async Task<IActionResult> DuyetLich(int id)
        {
            var lichHen = await _db.LichHen.FirstOrDefaultAsync(l => l.LichHenId == id);
            if (lichHen == null) return NotFound();

            // BẢO MẬT: Nếu là Doctor, chỉ được duyệt lịch của CHÍNH MÌNH
            if (User.IsInRole("Doctor"))
            {
                var bacSi = await GetCurrentBacSi();
                if (bacSi == null || lichHen.BacSiId != bacSi.BacSiId) return Forbid();
            }

            if (lichHen.TrangThai != TrangThaiLichHen.ChoDuyet)
            {
                TempData["error"] = "Lịch hẹn này không ở trạng thái chờ duyệt.";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra/Tạo bệnh nhân
            var benhNhan = await _db.BenhNhan.FirstOrDefaultAsync(b => b.SoDienThoai == lichHen.SoDienThoai);
            if (benhNhan == null)
            {
                benhNhan = new BenhNhan
                {
                    HoTen = lichHen.TenKhachHang,
                    SoDienThoai = lichHen.SoDienThoai,
                    Email = lichHen.Email,
                    NgayTao = DateTime.Now
                };
                _db.BenhNhan.Add(benhNhan);
                await _db.SaveChangesAsync();
            }

            // Tạo hồ sơ bệnh án tự động
            var hoSoMoi = new HoSoBenhAn
            {
                BenhNhanId = benhNhan.BenhNhanId,
                BacSiId = lichHen.BacSiId,
                NgayKham = DateTime.Now,
                TrangThai = TrangThaiHoSo.ChoKham,
                TrieuChung = lichHen.TrieuChung,
                LichHenId = lichHen.LichHenId,
                LichLamViecId = lichHen.LichLamViecId,
                KhungGioBatDau = lichHen.KhungGioBatDau
            };
            _db.HoSoBenhAn.Add(hoSoMoi);
            lichHen.TrangThai = TrangThaiLichHen.DaXacNhan;

            await _db.SaveChangesAsync();
            TempData["success"] = "Đã duyệt lịch và chuyển vào danh sách Chờ khám.";
            return RedirectToAction(nameof(Index));
        }

        // 3. THÊM MỚI LỊCH HẸN (GET)
        public async Task<IActionResult> Create()
        {
            PrepareViewData();

            // Lấy thông tin để khóa cứng ID nếu là Doctor
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

        // 4. THÊM MỚI LỊCH HẸN (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LichHen lichHen)
        {
            // LOGIC QUAN TRỌNG: Nếu là Doctor, ép ID là của chính mình
            if (User.IsInRole("Doctor"))
            {
                var bacSi = await GetCurrentBacSi();
                if (bacSi != null) lichHen.BacSiId = bacSi.BacSiId;
                ModelState.Remove("BacSi");
                ModelState.Remove("BacSiId");
            }

            if (ModelState.IsValid)
            {
                var llv = _db.LichLamViec.FirstOrDefault(l => l.LichLamViecId == lichHen.LichLamViecId);
                if (llv == null)
                {
                    ModelState.AddModelError("", "Ca làm việc không hợp lệ.");
                }
                else
                {
                    lichHen.BacSiId = llv.BacSiId;
                    lichHen.TrangThai = TrangThaiLichHen.ChoDuyet;
                    lichHen.ThoiGianDat = DateTime.Now;
                    _db.LichHen.Add(lichHen);
                    await _db.SaveChangesAsync();
                    TempData["success"] = "Đặt lịch thành công!";
                    return RedirectToAction(nameof(Index));
                }
            }

            // Nạp lại dữ liệu nếu lỗi
            if (User.IsInRole("Doctor"))
            {
                var bacSi = await GetCurrentBacSi();
                ViewBag.CurrentBacSiId = bacSi?.BacSiId;
                ViewBag.CurrentBacSiName = bacSi?.HoTen;
            }
            PrepareViewData(lichHen.BacSiId, lichHen.DichVuId);
            return View(lichHen);
        }

        // --- CÁC HÀM AJAX VÀ HELPER DỮ LIỆU ---

        private void PrepareViewData(object selectedBacSi = null, object selectedDichVu = null)
        {
            ViewData["BacSiId"] = new SelectList(_db.BacSi.OrderBy(b => b.HoTen), "BacSiId", "HoTen", selectedBacSi);
            ViewData["TrangThaiList"] = Enum.GetValues(typeof(TrangThaiLichHen)).Cast<TrangThaiLichHen>()
                .Select(e => new SelectListItem { Value = e.ToString(), Text = e.ToString() }).ToList();
        }

        [HttpGet]
        public IActionResult GetDichVuByBacSi(int bacSiId)
        {
            var bacSi = _db.BacSi.Include(b => b.ChuyenKhoa).FirstOrDefault(b => b.BacSiId == bacSiId);
            if (bacSi == null) return Json(new List<object>());
            var dichVuIds = _db.ChuyenKhoaDichVus.Where(ck => ck.ChuyenKhoaId == bacSi.ChuyenKhoaId).Select(ck => ck.DichVuId).ToList();
            var list = _db.DichVu.Where(dv => dichVuIds.Contains(dv.DichVuId) && dv.IsActive).Select(dv => new { value = dv.DichVuId, text = dv.TenDichVu }).ToList();
            return Json(list);
        }

        [HttpGet]
        public IActionResult GetLichLamViecByBacSi(int bacSiId)
        {
            var shifts = _db.LichLamViec.Where(l => l.IsActive && l.NgayLamViec.Date >= DateTime.Today && l.BacSiId == bacSiId).ToList();
            var result = shifts.Select(s => new {
                id = s.LichLamViecId,
                text = $"{s.NgayLamViec:dd/MM/yyyy} ({s.GioBatDau:hh\\:mm} - {s.GioKetThuc:hh\\:mm})",
                gioBatDau = s.GioBatDau.ToString(@"hh\:mm"),
                gioKetThuc = s.GioKetThuc.ToString(@"hh\:mm"),
                thoiLuongPhut = s.ThoiLuongKhungGioPhut,
                bookedSlots = _db.LichHen.Where(lh => lh.LichLamViecId == s.LichLamViecId && lh.TrangThai != TrangThaiLichHen.DaHuy).Select(lh => lh.KhungGioBatDau.ToString(@"hh\:mm")).ToList()
            }).ToList();
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTrangThaiAjax(int id, string trangThaiMoi)
        {
            var lichHen = await _db.LichHen.FindAsync(id);
            if (lichHen == null) return NotFound();
            if (User.IsInRole("Doctor"))
            {
                var bacSi = await GetCurrentBacSi();
                if (bacSi == null || lichHen.BacSiId != bacSi.BacSiId) return Forbid();
            }
            if (Enum.TryParse(trangThaiMoi, out TrangThaiLichHen status))
            {
                lichHen.TrangThai = status;
                await _db.SaveChangesAsync();
                return Ok();
            }
            return BadRequest();
        }

        // EDIT & DELETE (Bảo mật cho Doctor)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var lichHen = await _db.LichHen.FindAsync(id);
            if (User.IsInRole("Doctor"))
            {
                var bacSi = await GetCurrentBacSi();
                if (bacSi == null || lichHen.BacSiId != bacSi.BacSiId) return Forbid();
            }
            PrepareViewData(lichHen.BacSiId, lichHen.DichVuId);
            return View(lichHen);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            var lichHen = await _db.LichHen.Include(l => l.BacSi).FirstOrDefaultAsync(l => l.LichHenId == id);
            if (User.IsInRole("Doctor"))
            {
                var bacSi = await GetCurrentBacSi();
                if (bacSi == null || lichHen.BacSiId != bacSi.BacSiId) return Forbid();
            }
            return View(lichHen);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeletePOST(int id)
        {
            var obj = await _db.LichHen.FindAsync(id);
            _db.LichHen.Remove(obj);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}