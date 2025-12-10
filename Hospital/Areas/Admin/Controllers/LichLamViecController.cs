using Hospital.Data;
using Hospital.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LichLamViecController : Controller
    {
        private readonly ApplicationDbContext _db;

        public LichLamViecController(ApplicationDbContext db)
        {
            _db = db;
        }

        // Helper: Chuẩn bị SelectList cho Bác sĩ
        private void PrepareBacSiSelectList(object selectedBacSi = null)
        {
            ViewData["BacSiId"] = new SelectList(_db.BacSi, "BacSiId", "HoTen", selectedBacSi);
            // Thêm ViewBag cho bộ lọc Index (Text và Value đều là HoTen)
            ViewBag.BacSis = new SelectList(_db.BacSi.OrderBy(b => b.HoTen), "HoTen", "HoTen");
        }

        // GET: Admin/LichLamViec/Index (Xem danh sách)
        public IActionResult Index()
        {
            PrepareBacSiSelectList(); // Chuẩn bị SelectList cho bộ lọc

            var lichLamViecList = _db.LichLamViec.Include(l => l.BacSi).ToList();
            return View(lichLamViecList);
        }

        // GET: Admin/LichLamViec/Create (Tạo mới)
        public IActionResult Create()
        {
            PrepareBacSiSelectList();
            return View();
        }

        // POST: Admin/LichLamViec/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(LichLamViec lichLamViec)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra ngày làm việc không được trùng lặp cho cùng một Bác sĩ
                bool isDuplicate = _db.LichLamViec.Any(l =>
                    l.BacSiId == lichLamViec.BacSiId &&
                    l.NgayLamViec.Date == lichLamViec.NgayLamViec.Date
                );

                if (isDuplicate)
                {
                    ModelState.AddModelError("NgayLamViec", "Bác sĩ này đã có lịch làm việc được thiết lập cho ngày này rồi.");
                }
                // Thêm logic: Giờ kết thúc phải sau giờ bắt đầu
                else if (lichLamViec.GioKetThuc <= lichLamViec.GioBatDau)
                {
                    ModelState.AddModelError("GioKetThuc", "Giờ kết thúc phải sau giờ bắt đầu.");
                }
                else
                {
                    _db.LichLamViec.Add(lichLamViec);
                    _db.SaveChanges();
                    TempData["success"] = "Thêm lịch làm việc thành công!";
                    return RedirectToAction("Index");
                }
            }

            // Nếu lỗi, load lại SelectList và view
            PrepareBacSiSelectList(lichLamViec.BacSiId);
            return View(lichLamViec);
        }

        // GET: Admin/LichLamViec/Edit
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var lichLamViecFromDb = _db.LichLamViec.FirstOrDefault(l => l.LichLamViecId == id);

            if (lichLamViecFromDb == null)
            {
                return NotFound();
            }

            PrepareBacSiSelectList(lichLamViecFromDb.BacSiId);
            return View(lichLamViecFromDb);
        }

        // POST: Admin/LichLamViec/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(LichLamViec lichLamViec)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra trùng lặp (Loại trừ bản ghi đang chỉnh sửa)
                bool isDuplicate = _db.LichLamViec.Any(l =>
                    l.BacSiId == lichLamViec.BacSiId &&
                    l.NgayLamViec.Date == lichLamViec.NgayLamViec.Date &&
                    l.LichLamViecId != lichLamViec.LichLamViecId // Loại trừ ID hiện tại
                );

                if (isDuplicate)
                {
                    ModelState.AddModelError("NgayLamViec", "Bác sĩ này đã có lịch làm việc được thiết lập cho ngày này rồi.");
                }
                else if (lichLamViec.GioKetThuc <= lichLamViec.GioBatDau)
                {
                    ModelState.AddModelError("GioKetThuc", "Giờ kết thúc phải sau giờ bắt đầu.");
                }
                else
                {
                    _db.LichLamViec.Update(lichLamViec);
                    _db.SaveChanges();
                    TempData["success"] = "Cập nhật lịch làm việc thành công!";
                    return RedirectToAction("Index");
                }
            }

            // Nếu lỗi, load lại SelectList và view
            PrepareBacSiSelectList(lichLamViec.BacSiId);
            return View(lichLamViec);
        }

        // GET: Admin/LichLamViec/Delete
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            // Include Bác sĩ để hiển thị tên khi xác nhận xóa
            var lichLamViecFromDb = _db.LichLamViec.Include(l => l.BacSi).FirstOrDefault(l => l.LichLamViecId == id);

            if (lichLamViecFromDb == null)
            {
                return NotFound();
            }
            return View(lichLamViecFromDb);
        }

        // POST: Admin/LichLamViec/DeletePOST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePOST(int? id)
        {
            var obj = _db.LichLamViec.Find(id);
            if (obj == null)
            {
                return NotFound();
            }

            // KIỂM TRA QUAN TRỌNG: Ngăn xóa nếu có Lịch hẹn đang sử dụng ca làm việc này
            // Sử dụng .LichHens nếu bạn đã có Navigation Property trong Model
            // Nếu chưa, dùng code dưới đây
            bool hasAppointments = _db.LichHen.Any(lh => lh.LichLamViecId == id);

            if (hasAppointments)
            {
                TempData["error"] = "Không thể xóa ca làm việc này vì đã có Lịch hẹn được đặt!";
            }
            else
            {
                _db.LichLamViec.Remove(obj);
                _db.SaveChanges();
                TempData["success"] = "Xóa ca làm việc thành công!";
            }

            return RedirectToAction("Index");
        }
    }
}