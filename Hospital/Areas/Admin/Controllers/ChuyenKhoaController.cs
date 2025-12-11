using Hospital.Data;
using Hospital.Models;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ChuyenKhoaController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ChuyenKhoaController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: Admin/ChuyenKhoa/Index
        public IActionResult Index()
        {
            var objList = _db.ChuyenKhoa.ToList();
            return View(objList);
        }

        // GET: Admin/ChuyenKhoa/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/ChuyenKhoa/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
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