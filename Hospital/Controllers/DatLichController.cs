using Hospital.Data;
using Hospital.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hospital.Controllers
{
    public class DatLichController : Controller
    {
        private readonly ApplicationDbContext _db;

        public DatLichController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.BacSiList = await _db.BacSi.Where(b => b.IsActive).ToListAsync();
            return View(new LichHen());
        }

        // --- MỚI: API KIỂM TRA SỐ ĐIỆN THOẠI ---
        [HttpGet]
        public async Task<IActionResult> CheckPhoneNumber(string phone)
        {
            if (string.IsNullOrEmpty(phone)) return Json(new { found = false });

            // Kiểm tra xem số điện thoại đã có trong danh sách bệnh nhân chưa
            var patient = await _db.BenhNhan.FirstOrDefaultAsync(b => b.SoDienThoai == phone);

            if (patient != null)
            {
                return Json(new
                {
                    found = true,
                    name = patient.HoTen,
                    hasAccount = !string.IsNullOrEmpty(patient.IdentityUserId)
                });
            }

            return Json(new { found = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetBacSiInfo(int id)
        {
            var bacSi = await _db.BacSi.Include(b => b.ChuyenKhoa).FirstOrDefaultAsync(b => b.BacSiId == id);
            if (bacSi == null) return NotFound();
            return Json(new { hoTen = bacSi.HoTen, bangCap = bacSi.BangCap, hinhAnh = bacSi.HinhAnhUrl ?? "/images/default-doctor.jpg", chuyenKhoa = bacSi.ChuyenKhoa?.TenChuyenKhoa ?? "Đang cập nhật" });
        }

        [HttpGet]
        public async Task<IActionResult> GetDichVuByBacSi(int bacSiId)
        {
            var bacSi = await _db.BacSi.FirstOrDefaultAsync(b => b.BacSiId == bacSiId);
            if (bacSi == null) return Json(new List<object>());
            var list = await _db.ChuyenKhoaDichVus.Where(ck => ck.ChuyenKhoaId == bacSi.ChuyenKhoaId)
                .Select(ck => new { value = ck.DichVuId, text = ck.DichVu.TenDichVu }).ToListAsync();
            return Json(list);
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableSlots(int bacSiId, DateTime ngay)
        {
            var schedule = await _db.LichLamViec.FirstOrDefaultAsync(s => s.BacSiId == bacSiId && s.NgayLamViec.Date == ngay.Date && s.IsActive);
            if (schedule == null) return Json(new { success = false, message = "Bác sĩ không có lịch làm việc ngày này." });

            var bookedTimes = await _db.LichHen.Where(h => h.LichLamViecId == schedule.LichLamViecId && h.TrangThai != TrangThaiLichHen.DaHuy)
                .Select(h => h.KhungGioBatDau).ToListAsync();

            var slots = new List<object>();
            TimeSpan currentTime = schedule.GioBatDau;
            TimeSpan duration = TimeSpan.FromMinutes(schedule.ThoiLuongKhungGioPhut);

            while (currentTime + duration <= schedule.GioKetThuc)
            {
                slots.Add(new { time = currentTime.ToString(@"hh\:mm"), isBooked = bookedTimes.Contains(currentTime) });
                currentTime = currentTime.Add(duration);
            }
            return Json(new { success = true, slots, scheduleId = schedule.LichLamViecId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LichHen model)
        {
            if (ModelState.IsValid)
            {
                model.ThoiGianDat = DateTime.Now;
                model.TrangThai = TrangThaiLichHen.ChoDuyet;
                _db.LichHen.Add(model);
                await _db.SaveChangesAsync();
                return RedirectToAction("BookingSuccess");
            }
            ViewBag.BacSiList = await _db.BacSi.Where(b => b.IsActive).ToListAsync();
            return View("Index", model);
        }

        public IActionResult BookingSuccess() => View();
    }
}