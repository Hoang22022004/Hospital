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

        // --- 1. INDEX: TÌM KIẾM + LỌC NGÀY + PHÂN TRANG ---
        public async Task<IActionResult> Index(string searchString, DateTime? searchDate, int page = 1)
        {
            int pageSize = 10; // Số lượng bản ghi mỗi trang
            var query = _context.BenhNhan.AsNoTracking().AsQueryable();

            // A. Lọc theo ngày tạo (Ngày đăng ký bệnh nhân)
            if (searchDate.HasValue)
            {
                query = query.Where(x => x.NgayTao.Date == searchDate.Value.Date);
            }

            // B. Tìm kiếm theo Tên hoặc Số điện thoại
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim();
                query = query.Where(s => s.SoDienThoai.Contains(searchString) || s.HoTen.Contains(searchString));
            }

            // C. Sắp xếp: Bệnh nhân mới đăng ký hiển thị lên đầu
            query = query.OrderByDescending(x => x.NgayTao).ThenByDescending(x => x.BenhNhanId);

            // D. Xử lý phân trang
            int totalItems = await query.CountAsync();
            var data = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            // E. Truyền dữ liệu ra View
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.SearchString = searchString;
            ViewBag.SearchDate = searchDate?.ToString("yyyy-MM-dd"); // Trả về định dạng input date

            return View(data);
        }

        // --- 2. CREATE: KIỂM TRA TRÙNG SĐT & TỰ ĐỘNG GÁN NGÀY TẠO ---
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
                // Kiểm tra trùng Số điện thoại trước khi lưu
                bool isDuplicate = await _context.BenhNhan.AnyAsync(x => x.SoDienThoai == benhNhan.SoDienThoai);
                if (isDuplicate)
                {
                    ModelState.AddModelError("SoDienThoai", "Số điện thoại này đã tồn tại trong hệ thống!");
                    return View(benhNhan);
                }

                benhNhan.NgayTao = DateTime.Now; // Gán ngày tạo hệ thống
                _context.Add(benhNhan);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Thêm bệnh nhân mới thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(benhNhan);
        }

        // --- 3. EDIT: BẢO TOÀN NGÀY TẠO CŨ ---
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
                    // Truy xuất NgayTao cũ để không bị ghi đè thành null
                    var existingData = await _context.BenhNhan.AsNoTracking().FirstOrDefaultAsync(x => x.BenhNhanId == id);
                    if (existingData != null)
                    {
                        benhNhan.NgayTao = existingData.NgayTao;
                    }

                    _context.Update(benhNhan);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật thông tin bệnh nhân thành công!";
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

        // --- 4. DETAILS & DELETE ---
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            // Lấy kèm lịch sử khám nếu cần (Include HoSoBenhAn)
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
            if (benhNhan != null)
            {
                _context.BenhNhan.Remove(benhNhan);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa hồ sơ bệnh nhân khỏi hệ thống!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool BenhNhanExists(int id) => _context.BenhNhan.Any(e => e.BenhNhanId == id);
    }
}