using Hospital.Data;
using Hospital.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using static Hospital.Models.LichHen; // Để sử dụng enum TrangThaiLichHen
using System.Linq;
using System.Collections.Generic;

namespace Hospital.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LichHenController : Controller
    {
        private readonly ApplicationDbContext _db;

        public LichHenController(ApplicationDbContext db)
        {
            _db = db;
        }

        // ===============================================================
        // ACTION MỚI: DUYỆT LỊCH HẸN (CHỈ THÊM MỚI - KHÔNG SỬA CODE CŨ)
        // ===============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DuyetLich(int id)
        {
            // 1. Tìm lịch hẹn cần duyệt
            var lichHen = await _db.LichHen.FirstOrDefaultAsync(l => l.LichHenId == id);
            if (lichHen == null) return NotFound();

            // Chỉ duyệt những lịch đang ở trạng thái Chờ duyệt
            if (lichHen.TrangThai != TrangThaiLichHen.ChoDuyet)
            {
                TempData["error"] = "Lịch hẹn này không ở trạng thái chờ duyệt.";
                return RedirectToAction(nameof(Index));
            }

            // 2. KIỂM TRA BỆNH NHÂN (Dựa trên SĐT khách hàng cung cấp)
            var benhNhan = await _db.BenhNhan.FirstOrDefaultAsync(b => b.SoDienThoai == lichHen.SoDienThoai);

            if (benhNhan == null)
            {
                // Nếu chưa có trong danh sách bệnh nhân, tự động tạo mới
                benhNhan = new BenhNhan
                {
                    HoTen = lichHen.TenKhachHang,
                    SoDienThoai = lichHen.SoDienThoai,
                    Email = lichHen.Email,
                    NgayTao = DateTime.Now,
                    GhiChu = "Khách hàng mới tự động tạo từ lịch hẹn"
                };
                _db.BenhNhan.Add(benhNhan);
                await _db.SaveChangesAsync(); // Lưu để lấy BenhNhanId
            }

            // 3. TỰ ĐỘNG TẠO HỒ SƠ BỆNH ÁN (Trạng thái: Chờ khám)
            var hoSoMoi = new HoSoBenhAn
            {
                BenhNhanId = benhNhan.BenhNhanId,
                BacSiId = lichHen.BacSiId,
                NgayKham = DateTime.Now, // Ngày tạo hồ sơ
                TrangThai = TrangThaiHoSo.ChoKham,
                TrieuChung = lichHen.TrieuChung,
                LichHenId = lichHen.LichHenId,
                LichLamViecId = lichHen.LichLamViecId,
                // QUAN TRỌNG: Copy khung giờ từ lịch hẹn sang hồ sơ
                KhungGioBatDau = lichHen.KhungGioBatDau
            };
            _db.HoSoBenhAn.Add(hoSoMoi);

            // 4. CẬP NHẬT TRẠNG THÁI LỊCH HẸN SANG "ĐÃ XÁC NHẬN"
            lichHen.TrangThai = TrangThaiLichHen.DaXacNhan;

            await _db.SaveChangesAsync();

            TempData["success"] = $"Đã duyệt lịch cho khách {lichHen.TenKhachHang} và chuyển vào danh sách Chờ khám.";
            return RedirectToAction(nameof(Index));
        }

        // --- GIỮ NGUYÊN TOÀN BỘ CODE CŨ CỦA BẠN DƯỚI ĐÂY ---

        private void PrepareViewData(object selectedBacSi = null, object selectedDichVu = null)
        {
            ViewData["BacSiId"] = new SelectList(_db.BacSi.OrderBy(b => b.HoTen), "BacSiId", "HoTen", selectedBacSi);
            ViewData["TrangThaiList"] = Enum.GetValues(typeof(TrangThaiLichHen))
                                                .Cast<TrangThaiLichHen>()
                                                .Select(e => new SelectListItem
                                                {
                                                    Value = e.ToString(),
                                                    Text = e.ToString()
                                                }).ToList();
        }

        [HttpGet]
        public IActionResult GetDichVuByBacSi(int bacSiId)
        {
            var bacSi = _db.BacSi.Include(b => b.ChuyenKhoa).FirstOrDefault(b => b.BacSiId == bacSiId);
            if (bacSi == null) return Json(new List<SelectListItem>());

            var dichVuIds = _db.ChuyenKhoaDichVus
                      .Where(ckdv => ckdv.ChuyenKhoaId == bacSi.ChuyenKhoaId)
                      .Select(ckdv => ckdv.DichVuId).ToList();

            var dichVuList = _db.DichVu
                                .Where(dv => dichVuIds.Contains(dv.DichVuId) && dv.IsActive)
                                .OrderBy(dv => dv.TenDichVu)
                                .Select(dv => new SelectListItem { Value = dv.DichVuId.ToString(), Text = dv.TenDichVu })
                                .ToList();
            return Json(dichVuList);
        }

        [HttpGet]
        public IActionResult GetLichLamViecByBacSi(int bacSiId)
        {
            var shifts = _db.LichLamViec
                             .Where(l => l.IsActive && l.NgayLamViec.Date >= DateTime.Today && l.BacSiId == bacSiId)
                             .OrderBy(l => l.NgayLamViec).ThenBy(l => l.GioBatDau).ToList();

            var resultList = new List<object>();
            foreach (var shift in shifts)
            {
                var bookedSlots = _db.LichHen
                                          .Where(lh => lh.LichLamViecId == shift.LichLamViecId && lh.TrangThai != TrangThaiLichHen.DaHuy)
                                          .Select(lh => lh.KhungGioBatDau.ToString(@"hh\:mm")).ToList();

                resultList.Add(new
                {
                    id = shift.LichLamViecId,
                    text = $"{shift.NgayLamViec.ToString("dd/MM/yyyy")} ({shift.GioBatDau.ToString(@"hh\:mm")} - {shift.GioKetThuc.ToString(@"hh\:mm")})",
                    gioBatDau = shift.GioBatDau.ToString(@"hh\:mm"),
                    gioKetThuc = shift.GioKetThuc.ToString(@"hh\:mm"),
                    thoiLuongPhut = shift.ThoiLuongKhungGioPhut,
                    bookedSlots = bookedSlots
                });
            }
            return Json(resultList);
        }

        [HttpPost]
        public IActionResult UpdateTrangThaiAjax(int id, string trangThaiMoi)
        {
            var lichHen = _db.LichHen.Find(id);
            if (lichHen == null) return NotFound(new { success = false, message = "Không tìm thấy lịch hẹn." });

            if (Enum.TryParse(trangThaiMoi, out TrangThaiLichHen newStatus))
            {
                lichHen.TrangThai = newStatus;
                _db.LichHen.Update(lichHen);
                _db.SaveChanges();
                return Ok(new { success = true, message = $"Cập nhật trạng thái thành {newStatus.ToString()} thành công." });
            }
            return BadRequest(new { success = false, message = "Trạng thái không hợp lệ." });
        }

        public IActionResult Index()
        {
            PrepareViewData();
            var lichHenList = _db.LichHen.Include(l => l.BacSi).Include(l => l.DichVu).Include(l => l.LichLamViec).ToList();
            return View(lichHenList);
        }

        public IActionResult Create()
        {
            PrepareViewData();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(LichHen lichHen)
        {
            if (ModelState.IsValid)
            {
                var llv = _db.LichLamViec.Include(l => l.BacSi).FirstOrDefault(l => l.LichLamViecId == lichHen.LichLamViecId);
                if (llv == null || !llv.IsActive || llv.NgayLamViec.Date < DateTime.Today)
                    ModelState.AddModelError("LichLamViecId", "Ca làm việc này không hợp lệ hoặc đã kết thúc.");
                else if (_db.LichHen.Any(lh => lh.LichLamViecId == lichHen.LichLamViecId && lh.KhungGioBatDau == lichHen.KhungGioBatDau && lh.TrangThai != TrangThaiLichHen.DaHuy))
                    ModelState.AddModelError("KhungGioBatDau", $"Khung giờ {lichHen.KhungGioBatDau.ToString(@"hh\:mm")} đã có người đặt!");
                else if (lichHen.KhungGioBatDau < llv.GioBatDau || lichHen.KhungGioBatDau >= llv.GioKetThuc)
                    ModelState.AddModelError("KhungGioBatDau", "Khung giờ được chọn nằm ngoài phạm vi của ca làm việc này.");
                else
                {
                    lichHen.BacSiId = llv.BacSiId;
                    lichHen.TrangThai = TrangThaiLichHen.ChoDuyet;
                    lichHen.ThoiGianDat = DateTime.Now;
                    _db.LichHen.Add(lichHen);
                    _db.SaveChanges();
                    TempData["success"] = "Đặt lịch hẹn thành công! Đang chờ xác nhận.";
                    return RedirectToAction("Index");
                }
            }
            PrepareViewData(lichHen.BacSiId, lichHen.DichVuId);
            return View(lichHen);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0) return NotFound();
            var lichHenFromDb = _db.LichHen.FirstOrDefault(l => l.LichHenId == id);
            if (lichHenFromDb == null) return NotFound();
            PrepareViewData(lichHenFromDb.BacSiId, lichHenFromDb.DichVuId);
            return View(lichHenFromDb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(LichHen lichHen)
        {
            if (ModelState.IsValid)
            {
                var llv = _db.LichLamViec.FirstOrDefault(l => l.LichLamViecId == lichHen.LichLamViecId);
                if (llv == null) ModelState.AddModelError("LichLamViecId", "Ca làm việc không hợp lệ.");
                else if (_db.LichHen.Any(lh => lh.LichLamViecId == lichHen.LichLamViecId && lh.KhungGioBatDau == lichHen.KhungGioBatDau && lh.LichHenId != lichHen.LichHenId && lh.TrangThai != TrangThaiLichHen.DaHuy))
                    ModelState.AddModelError("KhungGioBatDau", $"Khung giờ {lichHen.KhungGioBatDau.ToString(@"hh\:mm")} đã có người đặt!");
                else if (lichHen.KhungGioBatDau < llv.GioBatDau || lichHen.KhungGioBatDau >= llv.GioKetThuc)
                    ModelState.AddModelError("KhungGioBatDau", "Khung giờ được chọn nằm ngoài phạm vi của ca làm việc này.");
                else
                {
                    lichHen.BacSiId = llv.BacSiId;
                    _db.LichHen.Update(lichHen);
                    _db.SaveChanges();
                    TempData["success"] = "Cập nhật lịch hẹn thành công!";
                    return RedirectToAction("Index");
                }
            }
            PrepareViewData(lichHen.BacSiId, lichHen.DichVuId);
            return View(lichHen);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0) return NotFound();
            var lichHenFromDb = _db.LichHen.Include(l => l.BacSi).Include(l => l.DichVu).Include(l => l.LichLamViec).FirstOrDefault(l => l.LichHenId == id);
            if (lichHenFromDb == null) return NotFound();
            return View(lichHenFromDb);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePOST(int? id)
        {
            var obj = _db.LichHen.Find(id);
            if (obj == null) return NotFound();
            _db.LichHen.Remove(obj);
            _db.SaveChanges();
            TempData["success"] = "Hủy lịch hẹn thành công!";
            return RedirectToAction("Index");
        }
    }
}