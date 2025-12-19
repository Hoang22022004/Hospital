using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Hospital.Data;
using Hospital.Models;

namespace Hospital.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BenhNhanController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BenhNhanController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- 1. INDEX: TÌM KIẾM + PHÂN TRANG + SẮP XẾP ---
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            int pageSize = 10; // Số khách hiển thị trên 1 trang

            var query = _context.BenhNhan.AsNoTracking().AsQueryable(); // AsNoTracking giúp query nhanh hơn

            // Tìm kiếm
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim();
                query = query.Where(s => s.SoDienThoai.Contains(searchString) || s.HoTen.Contains(searchString));
            }

            // Sắp xếp: Người mới tạo lên đầu
            query = query.OrderByDescending(x => x.BenhNhanId);

            // Xử lý phân trang
            int totalItems = await query.CountAsync();
            var data = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            // Truyền dữ liệu phân trang sang View
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.SearchString = searchString;

            return View(data);
        }

        // --- 2. CREATE: KIỂM TRA TRÙNG SĐT ---
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("HoTen,SoDienThoai,NgaySinh,GioiTinh,DiaChi,Email,TienSuBenh,GhiChu")] BenhNhan benhNhan)
        {
            if (ModelState.IsValid)
            {
                // CHECK TRÙNG SĐT: Logic quan trọng
                bool isDuplicate = await _context.BenhNhan.AnyAsync(x => x.SoDienThoai == benhNhan.SoDienThoai);
                if (isDuplicate)
                {
                    ModelState.AddModelError("SoDienThoai", "Số điện thoại này đã tồn tại trong hệ thống!");
                    return View(benhNhan);
                }

                benhNhan.NgayTao = DateTime.Now;
                _context.Add(benhNhan);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Thêm bệnh nhân mới thành công!"; // Hiển thị thông báo xanh
                return RedirectToAction(nameof(Index));
            }
            return View(benhNhan);
        }

        // --- 3. EDIT: GIỮ NGUYÊN NGÀY TẠO ---
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var benhNhan = await _context.BenhNhan.FindAsync(id);
            if (benhNhan == null) return NotFound();
            return View(benhNhan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BenhNhanId,HoTen,SoDienThoai,NgaySinh,GioiTinh,DiaChi,Email,TienSuBenh,GhiChu")] BenhNhan benhNhan)
        {
            if (id != benhNhan.BenhNhanId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy dữ liệu cũ để giữ lại NgayTao
                    var oldData = await _context.BenhNhan.AsNoTracking().FirstOrDefaultAsync(x => x.BenhNhanId == id);
                    if (oldData != null) benhNhan.NgayTao = oldData.NgayTao;

                    _context.Update(benhNhan);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật thông tin thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BenhNhanExists(benhNhan.BenhNhanId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(benhNhan);
        }

        // --- 4. DETAILS & DELETE (Giữ nguyên logic cơ bản) ---
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var benhNhan = await _context.BenhNhan.FirstOrDefaultAsync(m => m.BenhNhanId == id);
            if (benhNhan == null) return NotFound();
            return View(benhNhan);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var benhNhan = await _context.BenhNhan.FirstOrDefaultAsync(m => m.BenhNhanId == id);
            if (benhNhan == null) return NotFound();
            return View(benhNhan);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var benhNhan = await _context.BenhNhan.FindAsync(id);
            if (benhNhan != null) _context.BenhNhan.Remove(benhNhan);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã xóa hồ sơ bệnh nhân!";
            return RedirectToAction(nameof(Index));
        }

        private bool BenhNhanExists(int id) => _context.BenhNhan.Any(e => e.BenhNhanId == id);
    }
}