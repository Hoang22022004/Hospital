using Hospital.Data;
using Hospital.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using static Hospital.Models.LichHen; // Để sử dụng enum TrangThaiLichHen

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
            // Danh sách Bác sĩ (Chỉ cần SelectList cho Dropdown Bác sĩ)
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

            // LƯU Ý: KHÔNG CẦN CHUẨN BỊ LichLamViecId SelectList Ở ĐÂY NỮA
        }

        // Action mới: Trả về danh sách Ca làm việc trống dựa trên ID Bác sĩ (Phục vụ AJAX)
        [HttpGet]
        public IActionResult GetLichLamViecByBacSi(int bacSiId)
        {
            var availableShifts = _db.LichLamViec
                                     .Include(l => l.BacSi)
                                     .Where(l => l.IsActive
                                              && l.NgayLamViec.Date >= DateTime.Today
                                              && l.BacSiId == bacSiId)
                                     // Lọc các ca đã có lịch hẹn chưa hủy
                                     .Where(l => !_db.LichHen.Any(lh => lh.LichLamViecId == l.LichLamViecId
                                                                    && lh.TrangThai != TrangThaiLichHen.DaHuy))
                                     .OrderBy(l => l.NgayLamViec)
                                     .ThenBy(l => l.GioBatDau)
                                     .Select(l => new
                                     {
                                         id = l.LichLamViecId,
                                         // Format hiển thị: [Ngày] ([Giờ Bắt đầu] - [Giờ Kết thúc])
                                         text = $"{l.NgayLamViec.ToString("dd/MM/yyyy")} ({l.GioBatDau.ToString("HH:mm")} - {l.GioKetThuc.ToString("HH:mm")})"
                                     })
                                     .ToList();

            return Json(availableShifts);
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
            if (ModelState.IsValid)
            {
                // Lấy thông tin Ca làm việc từ ID
                var llv = _db.LichLamViec.Include(l => l.BacSi).FirstOrDefault(l => l.LichLamViecId == lichHen.LichLamViecId);

                if (llv == null || !llv.IsActive || llv.NgayLamViec.Date < DateTime.Today)
                {
                    ModelState.AddModelError("LichLamViecId", "Ca làm việc này không hợp lệ hoặc đã kết thúc.");
                }
                else
                {
                    // Đảm bảo BacSiId khớp với ca làm việc được chọn
                    lichHen.BacSiId = llv.BacSiId;

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
                // Cập nhật lại Khóa ngoại Bác sĩ dựa trên Ca làm việc mới được chọn (nếu có)
                var llv = _db.LichLamViec.FirstOrDefault(l => l.LichLamViecId == lichHen.LichLamViecId);
                if (llv != null)
                {
                    lichHen.BacSiId = llv.BacSiId;
                }

                _db.LichHen.Update(lichHen);
                _db.SaveChanges();
                TempData["success"] = "Cập nhật lịch hẹn thành công!";
                return RedirectToAction("Index");
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