using Hospital.Data;
using Hospital.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace Hospital.Areas.Admin.Controllers
{
    [Area("Admin")]
    // PHÂN QUYỀN CHUNG: Admin, Bác sĩ và Lễ tân đều được vào xem danh sách
    [Authorize(Roles = "Admin,Doctor,Receptionist")]
    public class ChuyenKhoaController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ChuyenKhoaController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: Admin/ChuyenKhoa/Index
        // Tất cả các quyền trên đều truy cập được hàm này
        public IActionResult Index()
        {
            var objList = _db.ChuyenKhoa.ToList();
            return View(objList);
        }

        // =========================================================================
        // CÁC HÀM THAY ĐỔI DỮ LIỆU: CHỈ DÀNH CHO ADMIN VÀ RECEPTIONIST
        // =========================================================================

        // GET: Admin/ChuyenKhoa/Create
        [Authorize(Roles = "Admin,Receptionist")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/ChuyenKhoa/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Receptionist")]
        public IActionResult Create(ChuyenKhoa obj)
        {
            // Kiểm tra trùng lặp tên
            if (_db.ChuyenKhoa.Any(c => c.TenChuyenKhoa == obj.TenChuyenKhoa))
            {
                ModelState.AddModelError("TenChuyenKhoa", "Tên chuyên khoa này đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                _db.ChuyenKhoa.Add(obj);
                _db.SaveChanges();
                TempData["success"] = "Thêm chuyên khoa thành công!";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        // GET: Admin/ChuyenKhoa/Edit
        [Authorize(Roles = "Admin,Receptionist")]
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var objFromDb = _db.ChuyenKhoa.Find(id);

            if (objFromDb == null)
            {
                return NotFound();
            }
            return View(objFromDb);
        }

        // POST: Admin/ChuyenKhoa/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Receptionist")]
        public IActionResult Edit(ChuyenKhoa obj)
        {
            // Kiểm tra trùng lặp tên (Loại trừ bản ghi đang chỉnh sửa)
            if (_db.ChuyenKhoa.Any(c => c.TenChuyenKhoa == obj.TenChuyenKhoa && c.ChuyenKhoaId != obj.ChuyenKhoaId))
            {
                ModelState.AddModelError("TenChuyenKhoa", "Tên chuyên khoa này đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                _db.ChuyenKhoa.Update(obj);
                _db.SaveChanges();
                TempData["success"] = "Cập nhật chuyên khoa thành công!";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        // GET: Admin/ChuyenKhoa/Delete
        [Authorize(Roles = "Admin,Receptionist")]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var objFromDb = _db.ChuyenKhoa.Find(id);

            if (objFromDb == null)
            {
                return NotFound();
            }
            return View(objFromDb);
        }

        // POST: Admin/ChuyenKhoa/DeletePOST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Receptionist")]
        public IActionResult DeletePOST(int? id)
        {
            var obj = _db.ChuyenKhoa.Find(id);
            if (obj == null)
            {
                return NotFound();
            }

            // KIỂM TRA QUAN TRỌNG: Ngăn xóa nếu chuyên khoa đang được sử dụng bởi Bác sĩ
            bool isUsed = _db.BacSi.Any(b => b.ChuyenKhoaId == id);

            if (isUsed)
            {
                TempData["error"] = "Không thể xóa chuyên khoa này vì đang có Bác sĩ sử dụng.";
            }
            else
            {
                _db.ChuyenKhoa.Remove(obj);
                _db.SaveChanges();
                TempData["success"] = "Xóa chuyên khoa thành công!";
            }

            return RedirectToAction("Index");
        }
    }
}