using Hospital.Data;
using Hospital.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using static Hospital.Models.LichHen; // Để sử dụng enum TrangThaiLichHen
using System.Linq; // Cần dùng cho các query LINQ

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

        // Helper: Chuẩn bị SelectList cho các trường
        private void PrepareViewData(object selectedBacSi = null, object selectedDichVu = null)
        {
            // Danh sách Bác sĩ
            ViewData["BacSiId"] = new SelectList(_db.BacSi.OrderBy(b => b.HoTen), "BacSiId", "HoTen", selectedBacSi);

            // Danh sách Dịch vụ
            ViewData["DichVuId"] = new SelectList(_db.DichVu, "DichVuId", "TenDichVu", selectedDichVu);

            // Danh sách trạng thái (dùng Enum)
            ViewData["TrangThaiList"] = Enum.GetValues(typeof(TrangThaiLichHen))
                                                .Cast<TrangThaiLichHen>()
                                                .Select(e => new SelectListItem
                                                {
                                                    Value = e.ToString(),
                                                    Text = e.ToString()
                                                }).ToList();
        }

        // Action mới: Trả về chi tiết Ca làm việc và các Khung giờ đã đặt (Phục vụ AJAX)
        [HttpGet]
        public IActionResult GetLichLamViecByBacSi(int bacSiId)
        {
            // Lấy tất cả các ca làm việc LỚN hợp lệ
            var shifts = _db.LichLamViec
                             .Where(l => l.IsActive
                                     && l.NgayLamViec.Date >= DateTime.Today
                                     && l.BacSiId == bacSiId)
                             .OrderBy(l => l.NgayLamViec)
                             .ThenBy(l => l.GioBatDau)
                             .ToList();

            var resultList = new List<object>();

            foreach (var shift in shifts)
            {
                // Lấy các khung giờ ĐÃ ĐẶT (KhungGioBatDau) cho ca làm việc này
                var bookedSlots = _db.LichHen
                                      .Where(lh => lh.LichLamViecId == shift.LichLamViecId
                                                && lh.TrangThai != TrangThaiLichHen.DaHuy)
                                      // Chuyển TimeSpan sang chuỗi "HH:mm" để JS dễ xử lý
                                      .Select(lh => lh.KhungGioBatDau.ToString(@"hh\:mm"))
                                      .ToList();

                // Chuẩn bị dữ liệu để JS xử lý phân chia khung giờ
                resultList.Add(new
                {
                    id = shift.LichLamViecId,
                    text = $"{shift.NgayLamViec.ToString("dd/MM/yyyy")} ({shift.GioBatDau.ToString(@"hh\:mm")} - {shift.GioKetThuc.ToString(@"hh\:mm")})",

                    // THÔNG TIN QUY TẮC PHÂN CHIA
                    gioBatDau = shift.GioBatDau.ToString(@"hh\:mm"),
                    gioKetThuc = shift.GioKetThuc.ToString(@"hh\:mm"),
                    thoiLuongPhut = shift.ThoiLuongKhungGioPhut, // Trường mới
                    bookedSlots = bookedSlots // Danh sách các slot đã bị chiếm
                });
            }

            return Json(resultList);
        }

        // ***************************************************************
        // ACTION MỚI: Cập nhật Trạng thái nhanh qua AJAX (Phục vụ trang Index)
        // ***************************************************************
        [HttpPost]
        public IActionResult UpdateTrangThaiAjax(int id, string trangThaiMoi)
        {
            var lichHen = _db.LichHen.Find(id);

            if (lichHen == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy lịch hẹn." });
            }

            // Chuyển đổi chuỗi sang Enum
            if (Enum.TryParse(trangThaiMoi, out TrangThaiLichHen newStatus))
            {
                lichHen.TrangThai = newStatus;
                _db.LichHen.Update(lichHen);
                _db.SaveChanges();

                // Trả về JSON thành công
                return Ok(new { success = true, message = $"Cập nhật trạng thái thành {newStatus.ToString()} thành công." });
            }
            else
            {
                return BadRequest(new { success = false, message = "Trạng thái không hợp lệ." });
            }
        }


        // GET: Admin/LichHen/Index (Xem danh sách)
        public IActionResult Index()
        {
            PrepareViewData();

            var lichHenList = _db.LichHen
                                 .Include(l => l.BacSi)
                                 .Include(l => l.DichVu)
                                 .Include(l => l.LichLamViec)
                                 .ToList();
            return View(lichHenList);
        }

        // GET: Admin/LichHen/Create (Tạo mới)
        public IActionResult Create()
        {
            PrepareViewData();
            return View();
        }

        // POST: Admin/LichHen/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(LichHen lichHen)
        {
            // Kiểm tra ModelState trước khi xử lý
            if (ModelState.IsValid)
            {
                // 1. Lấy thông tin Ca làm việc từ ID
                var llv = _db.LichLamViec.Include(l => l.BacSi).FirstOrDefault(l => l.LichLamViecId == lichHen.LichLamViecId);

                // 2. Kiểm tra tính hợp lệ của Ca làm việc lớn
                if (llv == null || !llv.IsActive || llv.NgayLamViec.Date < DateTime.Today)
                {
                    ModelState.AddModelError("LichLamViecId", "Ca làm việc này không hợp lệ hoặc đã kết thúc.");
                }

                // 3. KIỂM TRA TRÙNG LỊCH (Khung giờ nhỏ)
                else if (_db.LichHen.Any(lh =>
                         lh.LichLamViecId == lichHen.LichLamViecId &&
                         lh.KhungGioBatDau == lichHen.KhungGioBatDau && // So sánh Khung giờ Bắt đầu
                         lh.TrangThai != TrangThaiLichHen.DaHuy))
                {
                    ModelState.AddModelError("KhungGioBatDau", $"Khung giờ {lichHen.KhungGioBatDau.ToString(@"hh\:mm")} đã có người đặt!");
                }

                // 4. KIỂM TRA KhungGioBatDau nằm trong phạm vi GioBatDau và GioKetThuc của LLV
                // (Chỉ cần kiểm tra nếu không dùng JS để enforce)
                else if (lichHen.KhungGioBatDau < llv.GioBatDau || lichHen.KhungGioBatDau >= llv.GioKetThuc)
                {
                    ModelState.AddModelError("KhungGioBatDau", "Khung giờ được chọn nằm ngoài phạm vi của ca làm việc này.");
                }

                // 5. Thêm Lịch hẹn
                else
                {
                    // Cập nhật các trường tự động
                    lichHen.BacSiId = llv.BacSiId; // Đảm bảo BacSiId khớp với ca làm việc
                    lichHen.TrangThai = TrangThaiLichHen.ChoDuyet;
                    lichHen.ThoiGianDat = DateTime.Now;

                    _db.LichHen.Add(lichHen);
                    _db.SaveChanges();
                    TempData["success"] = "Đặt lịch hẹn thành công! Đang chờ xác nhận.";
                    return RedirectToAction("Index");
                }
            }

            // Nếu lỗi, load lại SelectList và view
            PrepareViewData(lichHen.BacSiId, lichHen.DichVuId);
            // LƯU Ý: Ở đây ta không cần tải lại LichLamViecId và KhungGioBatDau vì JS sẽ tự động xử lý.
            return View(lichHen);
        }

        // GET: Admin/LichHen/Edit
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0) return NotFound();
            var lichHenFromDb = _db.LichHen.FirstOrDefault(l => l.LichHenId == id);

            if (lichHenFromDb == null) return NotFound();

            // Truyền ID Bác sĩ và Dịch vụ hiện tại
            PrepareViewData(lichHenFromDb.BacSiId, lichHenFromDb.DichVuId);

            return View(lichHenFromDb);
        }

        // POST: Admin/LichHen/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(LichHen lichHen)
        {
            if (ModelState.IsValid)
            {
                // 1. Cập nhật lại Khóa ngoại Bác sĩ dựa trên Ca làm việc mới được chọn
                var llv = _db.LichLamViec.FirstOrDefault(l => l.LichLamViecId == lichHen.LichLamViecId);
                if (llv == null)
                {
                    ModelState.AddModelError("LichLamViecId", "Ca làm việc không hợp lệ.");
                }

                // 2. KIỂM TRA TRÙNG LỊCH (Khung giờ nhỏ) - Loại trừ bản ghi đang chỉnh sửa
                else if (_db.LichHen.Any(lh =>
                         lh.LichLamViecId == lichHen.LichLamViecId &&
                         lh.KhungGioBatDau == lichHen.KhungGioBatDau &&
                         lh.LichHenId != lichHen.LichHenId && // Loại trừ ID hiện tại
                         lh.TrangThai != TrangThaiLichHen.DaHuy))
                {
                    ModelState.AddModelError("KhungGioBatDau", $"Khung giờ {lichHen.KhungGioBatDau.ToString(@"hh\:mm")} đã có người đặt!");
                }

                else
                {
                    lichHen.BacSiId = llv.BacSiId; // Cập nhật BacSiId
                                                   // Giữ nguyên ThoiGianDat và TrangThai cũ nếu không có trường nào để cập nhật chúng trong View

                    _db.LichHen.Update(lichHen);
                    _db.SaveChanges();
                    TempData["success"] = "Cập nhật lịch hẹn thành công!";
                    return RedirectToAction("Index");
                }
            }

            // Nếu lỗi
            PrepareViewData(lichHen.BacSiId, lichHen.DichVuId);
            return View(lichHen);
        }

        // GET: Admin/LichHen/Delete
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0) return NotFound();

            var lichHenFromDb = _db.LichHen
                                   .Include(l => l.BacSi)
                                   .Include(l => l.DichVu)
                                   .Include(l => l.LichLamViec)
                                   .FirstOrDefault(l => l.LichHenId == id);

            if (lichHenFromDb == null) return NotFound();

            return View(lichHenFromDb);
        }

        // POST: Admin/LichHen/DeletePOST
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