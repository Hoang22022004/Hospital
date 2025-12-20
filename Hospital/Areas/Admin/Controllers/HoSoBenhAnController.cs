using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Hospital.Data;
using Hospital.Models;
using Microsoft.AspNetCore.Hosting;

namespace Hospital.Controllers
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
        // 1. DASHBOARD - QUẢN LÝ HÀNG ĐỢI
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var records = await _context.HoSoBenhAn
                .Include(h => h.BenhNhan)
                .Include(h => h.BacSi)
                .OrderByDescending(h => h.NgayKham)
                .ToListAsync();
            return View(records);
        }

        // ==========================================
        // 2. TIẾP NHẬN BỆNH NHÂN (LỄ TÂN)
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
                return RedirectToAction(nameof(Index));
            }
            return View(hoSo);
        }

        // ==========================================
        // 3. KHÁM BỆNH & KÊ ĐƠN (BÁC SĨ)
        // ==========================================
        public async Task<IActionResult> KhamBenh(int? id)
        {
            if (id == null) return NotFound();

            var hoSo = await _context.HoSoBenhAn
                .Include(h => h.BenhNhan)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (hoSo == null) return NotFound();

            // Chuyển trạng thái sang Đang khám
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
            IFormFile anhChinh, List<IFormFile> anhPhu,
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

            // Lưu Dịch vụ
            if (selectedDichVu != null)
            {
                foreach (var dvId in selectedDichVu)
                {
                    if (dvId > 0)
                        _context.ChiTietDichVu.Add(new ChiTietDichVu { HoSoBenhAnId = id, DichVuId = dvId });
                }
            }

            // Lưu Thuốc & TRỪ KHO
            if (selectedThuoc != null)
            {
                for (int i = 0; i < selectedThuoc.Length; i++)
                {
                    int thuocId = selectedThuoc[i];
                    if (thuocId > 0)
                    {
                        int qty = (soLuong != null && soLuong.Length > i) ? soLuong[i] : 1;

                        // Trừ tồn kho
                        var thuocStock = await _context.Thuoc.FindAsync(thuocId);
                        if (thuocStock != null)
                        {
                            thuocStock.SoLuongTon -= qty;
                            _context.Update(thuocStock);
                        }

                        _context.ChiTietDonThuoc.Add(new ChiTietDonThuoc
                        {
                            HoSoBenhAnId = id,
                            ThuocId = thuocId,
                            SoLuong = qty,
                            LieuDung = lieuDung[i]
                        });
                    }
                }
            }

            // Xử lý Ảnh
            string path = Path.Combine(_hostEnvironment.WebRootPath, @"images/da-lieu");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            if (anhChinh != null)
            {
                string fileName = "Main_" + Guid.NewGuid() + Path.GetExtension(anhChinh.FileName);
                using (var fs = new FileStream(Path.Combine(path, fileName), FileMode.Create)) { await anhChinh.CopyToAsync(fs); }
                _context.HinhAnhBenhAn.Add(new HinhAnhBenhAn { DuongDan = "/images/da-lieu/" + fileName, LaAnhChinh = true, HoSoBenhAnId = id });
            }

            if (anhPhu != null)
            {
                foreach (var item in anhPhu)
                {
                    string fileName = "Sub_" + Guid.NewGuid() + Path.GetExtension(item.FileName);
                    using (var fs = new FileStream(Path.Combine(path, fileName), FileMode.Create)) { await item.CopyToAsync(fs); }
                    _context.HinhAnhBenhAn.Add(new HinhAnhBenhAn { DuongDan = "/images/da-lieu/" + fileName, LaAnhChinh = false, HoSoBenhAnId = id });
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 4. THANH TOÁN & HÓA ĐƠN (THU NGÂN)
        // ==========================================
        public async Task<IActionResult> ThanhToan(int id)
        {
            // Lấy đầy đủ thông tin để tính tiền
            var hoSo = await _context.HoSoBenhAn
                .Include(h => h.BenhNhan)
                .Include(h => h.ChiTietDichVus).ThenInclude(d => d.DichVu)
                .Include(h => h.ChiTietDonThuocs).ThenInclude(t => t.Thuoc)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (hoSo == null) return NotFound();

            // Tính tổng tiền
            decimal tongDichVu = hoSo.ChiTietDichVus?.Sum(d => d.DichVu.Gia) ?? 0;
            decimal tongThuoc = hoSo.ChiTietDonThuocs?.Sum(t => t.SoLuong * t.Thuoc.GiaBan) ?? 0;

            ViewBag.TongTien = tongDichVu + tongThuoc;

            return View(hoSo);
        }

        [HttpPost]
        public async Task<IActionResult> XacNhanThanhToan(int id)
        {
            var hoSo = await _context.HoSoBenhAn.FindAsync(id);
            if (hoSo != null)
            {
                hoSo.TrangThai = TrangThaiHoSo.HoanThanh; // Chốt hồ sơ
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}