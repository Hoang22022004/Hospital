using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Hospital.Data;
using Hospital.Models;
using Microsoft.AspNetCore.Hosting;

namespace Hospital.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HoSoBenhAnController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public HoSoBenhAnController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // ==========================================
        // 1. DANH SÁCH HỒ SƠ & THỐNG KÊ DASHBOARD
        // ==========================================
        public async Task<IActionResult> Index(DateTime? searchDate, string searchString, TrangThaiHoSo? status, int page = 1)
        {
            int pageSize = 10;

            // Mặc định lấy ngày hôm nay nếu người dùng chưa chọn ngày
            if (searchDate == null)
            {
                searchDate = DateTime.Today;
            }

            var query = _context.HoSoBenhAn
                .Include(h => h.BenhNhan)
                .Include(h => h.BacSi)
                .AsQueryable();

            // Lọc theo ngày trước để lấy số liệu thống kê của ngày đang chọn
            query = query.Where(h => h.NgayKham.Date == searchDate.Value.Date);

            // --- THỐNG KÊ SỐ LƯỢNG (Dashboard Stats) ---
            ViewBag.CountChoKham = await query.CountAsync(x => x.TrangThai == TrangThaiHoSo.ChoKham);
            ViewBag.CountDangKham = await query.CountAsync(x => x.TrangThai == TrangThaiHoSo.DangKham);
            ViewBag.CountChoThanhToan = await query.CountAsync(x => x.TrangThai == TrangThaiHoSo.ChoThanhToan);
            ViewBag.CountHoanThanh = await query.CountAsync(x => x.TrangThai == TrangThaiHoSo.HoanThanh);

            // --- CÁC BỘ LỌC TÌM KIẾM ---
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim();
                query = query.Where(h => h.BenhNhan.HoTen.Contains(searchString) ||
                                         h.BenhNhan.SoDienThoai.Contains(searchString));
            }

            if (status.HasValue)
            {
                query = query.Where(h => h.TrangThai == status.Value);
            }

            // --- XỬ LÝ PHÂN TRANG ---
            int totalItems = await query.CountAsync();
            var records = await query
                .OrderByDescending(h => h.NgayKham)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Truyền dữ liệu sang View để giữ trạng thái giao diện
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.SearchDate = searchDate.Value.ToString("yyyy-MM-dd");
            ViewBag.SearchString = searchString;
            ViewBag.Status = status;

            return View(records);
        }

        // ==========================================
        // 2. CHI TIẾT BỆNH ÁN & TÍNH TIỀN
        // ==========================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var hoSo = await _context.HoSoBenhAn
                .Include(h => h.BenhNhan)
                .Include(h => h.BacSi)
                .Include(h => h.ChiTietDichVus).ThenInclude(d => d.DichVu)
                .Include(h => h.ChiTietDonThuocs).ThenInclude(t => t.Thuoc)
                .Include(h => h.HinhAnhBenhAns)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (hoSo == null) return NotFound();

            // Tính toán tiền (Sử dụng .Gia cho dịch vụ theo yêu cầu)
            decimal tongDichVu = hoSo.ChiTietDichVus?.Sum(d => d.DichVu.Gia) ?? 0;
            decimal tongThuoc = hoSo.ChiTietDonThuocs?.Sum(t => t.SoLuong * t.Thuoc.GiaBan) ?? 0;

            // Ép kiểu decimal rõ ràng để tránh lỗi ToString("N0") tại View
            ViewBag.TongDichVu = (decimal)tongDichVu;
            ViewBag.TongThuoc = (decimal)tongThuoc;
            ViewBag.TongTien = (decimal)(tongDichVu + tongThuoc);

            return View(hoSo);
        }

        // ==========================================
        // 3. TIẾP NHẬN BỆNH NHÂN (CREATE)
        // ==========================================
        public IActionResult Create(int? benhNhanId)
        {
            ViewData["BenhNhanId"] = new SelectList(_context.BenhNhan, "BenhNhanId", "HoTen", benhNhanId);
            ViewData["BacSiId"] = new SelectList(_context.BacSi, "BacSiId", "HoTen");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HoSoBenhAn hoSo)
        {
            if (ModelState.IsValid)
            {
                hoSo.TrangThai = TrangThaiHoSo.ChoKham;
                hoSo.NgayKham = DateTime.Now;
                _context.Add(hoSo);
                await _context.SaveChangesAsync();
                TempData["success"] = "Tiếp nhận thành công.";
                return RedirectToAction(nameof(Index));
            }
            return View(hoSo);
        }

        // ==========================================
        // 4. BÁC SĨ KHÁM BỆNH (NHẬP LIỆU LÂM SÀNG)
        // ==========================================
        public async Task<IActionResult> KhamBenh(int? id)
        {
            if (id == null) return NotFound();
            var hoSo = await _context.HoSoBenhAn.Include(h => h.BenhNhan).FirstOrDefaultAsync(m => m.Id == id);
            if (hoSo == null) return NotFound();

            if (hoSo.TrangThai == TrangThaiHoSo.ChoKham)
            {
                hoSo.TrangThai = TrangThaiHoSo.DangKham;
                _context.Update(hoSo);
                await _context.SaveChangesAsync();
            }
            ViewBag.DichVuList = await _context.DichVu.ToListAsync();
            ViewBag.ThuocList = await _context.Thuoc.ToListAsync();
            return View(hoSo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HoanTatKham(int id, HoSoBenhAn hoSoUpdate,
            IFormFile? anhChinh, List<IFormFile>? anhPhu,
            int[] selectedDichVu, int[] selectedThuoc, int[] soLuong, string[] lieuDung)
        {
            if (id != hoSoUpdate.Id) return NotFound();
            var record = await _context.HoSoBenhAn.FindAsync(id);
            if (record == null) return NotFound();

            // Cập nhật thông tin lâm sàng
            record.ChanDoan = hoSoUpdate.ChanDoan;
            record.TrieuChung = hoSoUpdate.TrieuChung;
            record.TinhTrangDa = hoSoUpdate.TinhTrangDa;
            record.ViTriTonThuong = hoSoUpdate.ViTriTonThuong;
            record.MucDo = hoSoUpdate.MucDo;
            record.LoiDan = hoSoUpdate.LoiDan;
            record.NgayTaiKham = hoSoUpdate.NgayTaiKham;
            record.TrangThai = TrangThaiHoSo.ChoThanhToan;

            // Lưu Dịch vụ kỹ thuật
            if (selectedDichVu != null)
            {
                foreach (var dvId in selectedDichVu)
                    _context.ChiTietDichVu.Add(new ChiTietDichVu { HoSoBenhAnId = id, DichVuId = dvId });
            }

            // Lưu Đơn thuốc & Trừ tồn kho thực tế
            if (selectedThuoc != null)
            {
                for (int i = 0; i < selectedThuoc.Length; i++)
                {
                    int tId = selectedThuoc[i];
                    int qty = (soLuong != null && soLuong.Length > i) ? soLuong[i] : 1;
                    var tStock = await _context.Thuoc.FindAsync(tId);
                    if (tStock != null) tStock.SoLuongTon -= qty;

                    _context.ChiTietDonThuoc.Add(new ChiTietDonThuoc
                    {
                        HoSoBenhAnId = id,
                        ThuocId = tId,
                        SoLuong = qty,
                        LieuDung = (lieuDung != null && lieuDung.Length > i) ? lieuDung[i] : ""
                    });
                }
            }

            // Xử lý Hình ảnh bệnh án lâm sàng
            string path = Path.Combine(_hostEnvironment.WebRootPath, @"images/da-lieu");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            if (anhChinh != null)
            {
                string fn = "Main_" + Guid.NewGuid() + Path.GetExtension(anhChinh.FileName);
                using (var fs = new FileStream(Path.Combine(path, fn), FileMode.Create)) await anhChinh.CopyToAsync(fs);
                _context.HinhAnhBenhAn.Add(new HinhAnhBenhAn { DuongDan = "/images/da-lieu/" + fn, LaAnhChinh = true, HoSoBenhAnId = id });
            }

            if (anhPhu != null)
            {
                foreach (var item in anhPhu)
                {
                    string fn = "Sub_" + Guid.NewGuid() + Path.GetExtension(item.FileName);
                    using (var fs = new FileStream(Path.Combine(path, fn), FileMode.Create)) await item.CopyToAsync(fs);
                    _context.HinhAnhBenhAn.Add(new HinhAnhBenhAn { DuongDan = "/images/da-lieu/" + fn, LaAnhChinh = false, HoSoBenhAnId = id });
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 5. THANH TOÁN (Tái sử dụng logic nạp dữ liệu từ Details)
        // ==========================================
        public async Task<IActionResult> ThanhToan(int id)
        {
            return await Details(id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhanThanhToan(int id)
        {
            var hoSo = await _context.HoSoBenhAn.FindAsync(id);
            if (hoSo != null)
            {
                hoSo.TrangThai = TrangThaiHoSo.HoanThanh;
                await _context.SaveChangesAsync();
                TempData["success"] = "Thanh toán thành công. Hồ sơ đã hoàn tất và đóng.";
            }
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 6. XÓA HỒ SƠ
        // ==========================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hoSo = await _context.HoSoBenhAn.FindAsync(id);
            if (hoSo != null)
            {
                _context.HoSoBenhAn.Remove(hoSo);
                await _context.SaveChangesAsync();
                TempData["success"] = "Đã xóa hồ sơ bệnh án thành công.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}