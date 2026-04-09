using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Hospital.Data;
using Hospital.Models;
using Microsoft.AspNetCore.Authorization;

namespace Hospital.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Doctor,Receptionist")] // Cả 3 quyền đều có thể vào xem danh sách
    public class BenhNhanController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BenhNhanController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- 1. INDEX: DANH SÁCH BỆNH NHÂN ---
        public async Task<IActionResult> Index(string searchString, DateTime? searchDate, int page = 1)
        {
            int pageSize = 10;
            var query = _context.BenhNhan.AsNoTracking().AsQueryable();

            if (searchDate.HasValue)
            {
                query = query.Where(x => x.NgayTao.Date == searchDate.Value.Date);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim();
                query = query.Where(s => s.SoDienThoai.Contains(searchString) || s.HoTen.Contains(searchString));
            }

            query = query.OrderByDescending(x => x.NgayTao).ThenByDescending(x => x.BenhNhanId);

            int totalItems = await query.CountAsync();
            var data = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.SearchString = searchString;
            ViewBag.SearchDate = searchDate?.ToString("yyyy-MM-dd");

            return View(data);
        }

        // --- 2. CREATE: TIẾP NHẬN BỆNH NHÂN MỚI ---
        [Authorize(Roles = "Admin,Receptionist")]
        public IActionResult Create(string phone)
        {
            // Nếu có SĐT truyền từ trang Đặt lịch sang thì tự điền vào Model
            var model = new BenhNhan();
            if (!string.IsNullOrEmpty(phone))
            {
                model.SoDienThoai = phone;
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Create([Bind("HoTen,SoDienThoai,NgaySinh,GioiTinh,DiaChi,Email,TienSuBenh,GhiChu")] BenhNhan benhNhan)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra trùng SĐT
                bool isDuplicate = await _context.BenhNhan.AnyAsync(x => x.SoDienThoai == benhNhan.SoDienThoai);
                if (isDuplicate)
                {
                    ModelState.AddModelError("SoDienThoai", "Số điện thoại này đã tồn tại trên hệ thống!");
                    return View(benhNhan);
                }

                benhNhan.NgayTao = DateTime.Now;
                _context.Add(benhNhan);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm bệnh nhân mới thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(benhNhan);
        }

        // --- 3. EDIT: CHỈNH SỬA HỒ SƠ ---
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var benhNhan = await _context.BenhNhan.FindAsync(id);
            if (benhNhan == null) return NotFound();
            return View(benhNhan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Edit(int id, [Bind("BenhNhanId,HoTen,SoDienThoai,NgaySinh,GioiTinh,DiaChi,Email,TienSuBenh,GhiChu")] BenhNhan benhNhan)
        {
            if (id != benhNhan.BenhNhanId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Giữ nguyên ngày tạo ban đầu
                    var existingData = await _context.BenhNhan.AsNoTracking().FirstOrDefaultAsync(x => x.BenhNhanId == id);
                    if (existingData != null) benhNhan.NgayTao = existingData.NgayTao;

                    _context.Update(benhNhan);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật thành công!";
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

        // --- 4. DETAILS & LỊCH SỬ KHÁM ---
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var benhNhan = await _context.BenhNhan.FirstOrDefaultAsync(m => m.BenhNhanId == id);
            if (benhNhan == null) return NotFound();
            return View(benhNhan);
        }

        public async Task<IActionResult> LichSuKham(int? id)
        {
            if (id == null) return NotFound();
            var benhNhan = await _context.BenhNhan.FirstOrDefaultAsync(m => m.BenhNhanId == id);
            if (benhNhan == null) return NotFound();

            // 1. Lấy danh sách hồ sơ bệnh án (KHÔNG DÙNG INCLUDE BENHLY ĐỂ TRÁNH LỖI)
            var lichSu = await _context.HoSoBenhAn
                .Include(h => h.BacSi)
                .Where(h => h.BenhNhanId == id)
                .OrderByDescending(h => h.NgayKham)
                .ToListAsync();

            // 2. Lấy danh sách Bệnh lý bỏ vào ViewBag để View tra cứu Tên từ Mã (ChanDoan)
            ViewBag.BenhLyList = await _context.BenhLy.AsNoTracking().ToListAsync();

            ViewBag.TenBenhNhan = benhNhan.HoTen;
            ViewBag.IdBenhNhan = id;
            return View(lichSu);
        }

        // --- 5. API CHECK SĐT (DÙNG CHO AJAX TRÊN VIEW) ---
        [HttpGet]
        public async Task<IActionResult> CheckPhoneNumber(string phone)
        {
            if (string.IsNullOrEmpty(phone)) return Json(new { found = false });

            var patient = await _context.BenhNhan
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.SoDienThoai == phone);

            if (patient != null)
            {
                return Json(new
                {
                    found = true,
                    name = patient.HoTen,
                    email = patient.Email,
                    id = patient.BenhNhanId
                });
            }
            return Json(new { found = false });
        }

        // --- 6. DELETE ---
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var benhNhan = await _context.BenhNhan.FirstOrDefaultAsync(m => m.BenhNhanId == id);
            if (benhNhan == null) return NotFound();
            return View(benhNhan);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var benhNhan = await _context.BenhNhan.FindAsync(id);
            if (benhNhan != null)
            {
                _context.BenhNhan.Remove(benhNhan);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa hồ sơ bệnh nhân!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool BenhNhanExists(int id) => _context.BenhNhan.Any(e => e.BenhNhanId == id);
    }
}