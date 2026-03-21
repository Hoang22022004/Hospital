using Hospital.Data;
using Hospital.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Hospital.Helpers;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace Hospital.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Receptionist,Doctor,Customer")]
    public class HoSoBenhAnController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IConfiguration _configuration;
        private readonly VnpayConfig _vnpayConfig;

        public HoSoBenhAnController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment, IConfiguration configuration, IOptions<VnpayConfig> vnpayOptions)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
            _configuration = configuration;
            _vnpayConfig = vnpayOptions.Value;
        }

        // --- HÀM HELPER: Lấy ID Bác sĩ từ tài khoản đang đăng nhập ---
        private async Task<BacSi?> GetCurrentBacSi()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return await _context.BacSi.FirstOrDefaultAsync(u => u.IdentityUserId == userId);
        }

        // ===============================================================
        // 1. DANH SÁCH HỒ SƠ & THỐNG KÊ
        // ===============================================================
        public async Task<IActionResult> Index(DateTime? searchDate, string searchString, TrangThaiHoSo? status, int page = 1)
        {
            int pageSize = 10;
            if (searchDate == null) searchDate = DateTime.Today;

            var query = _context.HoSoBenhAn
                .Include(h => h.BenhNhan)
                .Include(h => h.BacSi)
                .Include(h => h.LichHen)
                .AsQueryable();

            if (User.IsInRole("Doctor"))
            {
                var currentDoc = await GetCurrentBacSi();
                if (currentDoc != null)
                {
                    query = query.Where(h => h.BacSiId == currentDoc.BacSiId);
                }
            }

            query = query.Where(h => h.NgayKham.Date == searchDate.Value.Date);

            ViewBag.CountChoKham = await query.CountAsync(x => x.TrangThai == TrangThaiHoSo.ChoKham);
            ViewBag.CountDangKham = await query.CountAsync(x => x.TrangThai == TrangThaiHoSo.DangKham);
            ViewBag.CountChoThanhToan = await query.CountAsync(x => x.TrangThai == TrangThaiHoSo.ChoThanhToan);
            ViewBag.CountHoanThanh = await query.CountAsync(x => x.TrangThai == TrangThaiHoSo.HoanThanh);

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

            int totalItems = await query.CountAsync();
            var records = await query
                .OrderByDescending(h => h.NgayKham)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.SearchDate = searchDate.Value.ToString("yyyy-MM-dd");
            ViewBag.SearchString = searchString;
            ViewBag.Status = status;

            return View(records);
        }

        // ===============================================================
        // 2. CHI TIẾT BỆNH ÁN & TÍNH TIỀN
        // ===============================================================
        [Authorize(Roles = "Admin,Receptionist,Doctor,Customer")]
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

            decimal tongDichVu = hoSo.ChiTietDichVus?.Sum(d => d.DichVu.Gia) ?? 0;
            decimal tongThuoc = hoSo.ChiTietDonThuocs?.Sum(t => (decimal)(t.SoLuong) * (t.Thuoc?.GiaBan ?? 0)) ?? 0;

            ViewBag.TongDichVu = tongDichVu;
            ViewBag.TongThuoc = tongThuoc;
            ViewBag.TongTien = tongDichVu + tongThuoc;

            return View(hoSo);
        }

        // ===============================================================
        // 3. TIẾP NHẬN BỆNH NHÂN (ADMIN & RECEPTIONIST)
        // ===============================================================
        [Authorize(Roles = "Admin,Receptionist")]
        public IActionResult Create(int? benhNhanId)
        {
            var todayAppointments = _context.LichHen
                .Include(l => l.BacSi)
                .Include(l => l.LichLamViec)
                .Where(l => l.LichLamViec.NgayLamViec.Date == DateTime.Today && l.TrangThai == TrangThaiLichHen.DaXacNhan)
                .Select(l => new { Id = l.LichHenId, Text = $"[{l.KhungGioBatDau:hh\\:mm}] {l.TenKhachHang} - BS.{l.BacSi.HoTen}" })
                .ToList();
            ViewBag.TodayAppointments = new SelectList(todayAppointments, "Id", "Text");

            ViewData["BenhNhanId"] = new SelectList(_context.BenhNhan, "BenhNhanId", "HoTen", benhNhanId);
            ViewData["BacSiId"] = new SelectList(_context.BacSi, "BacSiId", "HoTen");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Create(HoSoBenhAn hoSo, int DichVuId)
        {
            if (ModelState.IsValid)
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var benhNhan = await _context.BenhNhan.FindAsync(hoSo.BenhNhanId);
                        var lichHenTuDong = new LichHen
                        {
                            TenKhachHang = benhNhan.HoTen,
                            SoDienThoai = benhNhan.SoDienThoai,
                            Email = benhNhan.Email,
                            BacSiId = hoSo.BacSiId,
                            LichLamViecId = hoSo.LichLamViecId ?? 0,
                            KhungGioBatDau = hoSo.KhungGioBatDau ?? TimeSpan.Zero,
                            DichVuId = DichVuId,
                            TrangThai = TrangThaiLichHen.DaXacNhan,
                            ThoiGianDat = DateTime.Now,
                            TrieuChung = hoSo.TrieuChung
                        };

                        _context.LichHen.Add(lichHenTuDong);
                        await _context.SaveChangesAsync();

                        hoSo.LichHenId = lichHenTuDong.LichHenId;
                        hoSo.TrangThai = TrangThaiHoSo.ChoKham;
                        hoSo.NgayKham = DateTime.Now;

                        _context.Add(hoSo);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        TempData["success"] = "Tiếp nhận bệnh nhân thành công!";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError("", "Lỗi xử lý: " + ex.Message);
                    }
                }
            }
            ViewData["BenhNhanId"] = new SelectList(_context.BenhNhan, "BenhNhanId", "HoTen", hoSo.BenhNhanId);
            ViewData["BacSiId"] = new SelectList(_context.BacSi, "BacSiId", "HoTen", hoSo.BacSiId);
            return View(hoSo);
        }

        // ===============================================================
        // 4. BÁC SĨ KHÁM BỆNH (DOCTOR)
        // ===============================================================
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> KhamBenh(int? id)
        {
            if (id == null) return NotFound();
            var hoSo = await _context.HoSoBenhAn.Include(h => h.BenhNhan).FirstOrDefaultAsync(m => m.Id == id);
            if (hoSo == null) return NotFound();

            var currentDoc = await GetCurrentBacSi();
            if (currentDoc == null || hoSo.BacSiId != currentDoc.BacSiId)
            {
                return Forbid();
            }

            if (hoSo.TrangThai == TrangThaiHoSo.ChoKham)
            {
                hoSo.TrangThai = TrangThaiHoSo.DangKham;
                _context.Update(hoSo);
                await _context.SaveChangesAsync();
            }
            ViewBag.DichVuList = await _context.DichVu.Where(d => d.IsActive).ToListAsync();
            ViewBag.ThuocList = await _context.Thuoc.Select(t => new {
                t.ThuocId,
                t.TenThuoc,
                PhanLoaiName = t.PhanLoai == PhanLoaiThuoc.ThuocUong ? "Thuốc uống" :
                               t.PhanLoai == PhanLoaiThuoc.ThuocBoi ? "Thuốc bôi ngoài da" :
                               t.PhanLoai == PhanLoaiThuoc.DuocMyPham ? "Dược mỹ phẩm" : "Vật tư y tế",
                PhanLoaiEnum = t.PhanLoai.ToString()
            }).ToListAsync();
            return View(hoSo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> HoanTatKham(int id, HoSoBenhAn hoSoUpdate,
            IFormFile? anhChinh, List<IFormFile>? anhPhu,
            int[] selectedDichVu, int[] selectedThuoc,
            double[] soLuong,
            string[] lieuSang, string[] lieuTrua, string[] lieuChieu, string[] lieuToi,
            int[] soNgayDung, string[] ghiChuThuoc)
        {
            if (id != hoSoUpdate.Id) return NotFound();

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var record = await _context.HoSoBenhAn.FindAsync(id);
                    if (record == null) return NotFound();

                    record.ChanDoan = hoSoUpdate.ChanDoan;
                    record.TrieuChung = hoSoUpdate.TrieuChung;
                    record.TinhTrangDa = hoSoUpdate.TinhTrangDa;
                    record.ViTriTonThuong = hoSoUpdate.ViTriTonThuong;
                    record.MucDo = hoSoUpdate.MucDo;
                    record.LoiDan = hoSoUpdate.LoiDan;
                    record.NgayTaiKham = hoSoUpdate.NgayTaiKham;
                    record.TrangThai = TrangThaiHoSo.ChoThanhToan;

                    if (selectedDichVu != null)
                        foreach (var dvId in selectedDichVu)
                            _context.ChiTietDichVu.Add(new ChiTietDichVu { HoSoBenhAnId = id, DichVuId = dvId });

                    if (selectedThuoc != null)
                    {
                        for (int i = 0; i < selectedThuoc.Length; i++)
                        {
                            int tId = selectedThuoc[i];
                            double qty = (soLuong != null && soLuong.Length > i) ? soLuong[i] : 0;
                            int days = (soNgayDung != null && soNgayDung.Length > i) ? soNgayDung[i] : 1;

                            var tStock = await _context.Thuoc.FindAsync(tId);
                            if (tStock != null) tStock.SoLuongTon -= (int)qty;

                            _context.ChiTietDonThuoc.Add(new ChiTietDonThuoc
                            {
                                HoSoBenhAnId = id,
                                ThuocId = tId,
                                SoLuong = qty,
                                LieuSang = (lieuSang != null && lieuSang.Length > i) ? lieuSang[i] : null,
                                LieuTrua = (lieuTrua != null && lieuTrua.Length > i) ? lieuTrua[i] : null,
                                LieuChieu = (lieuChieu != null && lieuChieu.Length > i) ? lieuChieu[i] : null,
                                LieuToi = (lieuToi != null && lieuToi.Length > i) ? lieuToi[i] : null,
                                SoNgayDung = days,
                                GhiChu = (ghiChuThuoc != null && ghiChuThuoc.Length > i) ? ghiChuThuoc[i] : null
                            });
                        }
                    }

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
                    await transaction.CommitAsync();

                    TempData["success"] = "Hoàn tất khám bệnh!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["error"] = "Lỗi khi lưu dữ liệu: " + ex.Message;
                    return RedirectToAction("KhamBenh", new { id = id });
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Receptionist,Customer")]
        public async Task<IActionResult> XacNhanThanhToan(int id)
        {
            var hoSo = await _context.HoSoBenhAn
                .Include(h => h.LichHen)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (hoSo != null)
            {
                hoSo.TrangThai = TrangThaiHoSo.HoanThanh;
                if (hoSo.LichHen != null)
                {
                    hoSo.LichHen.TrangThai = TrangThaiLichHen.HoanThanh;
                }

                await _context.SaveChangesAsync();
                TempData["success"] = "Xác nhận thanh toán thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hoSo = await _context.HoSoBenhAn.FindAsync(id);
            if (hoSo != null)
            {
                _context.HoSoBenhAn.Remove(hoSo);
                await _context.SaveChangesAsync();
                TempData["success"] = "Đã xóa hồ sơ.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> HuyHoSo(int id)
        {
            var hoSo = await _context.HoSoBenhAn.Include(h => h.LichHen).FirstOrDefaultAsync(h => h.Id == id);
            if (hoSo == null) return NotFound();

            hoSo.TrangThai = (TrangThaiHoSo)99;
            if (hoSo.LichHen != null) hoSo.LichHen.TrangThai = TrangThaiLichHen.DaHuy;

            await _context.SaveChangesAsync();
            TempData["success"] = "Đã hủy hồ sơ thành công.";
            return RedirectToAction(nameof(Index));
        }

        // ===============================================================
        // 7. CÁC HÀM AJAX ĐỒNG BỘ DỮ LIỆU (ĐÃ THÊM ĐIỀU KIỆN CHUYÊN KHOA)
        // ===============================================================

        [HttpGet]
        public async Task<IActionResult> GetDoctorAdmissionData(int bacSiId)
        {
            var bacSi = await _context.BacSi.Include(b => b.ChuyenKhoa).FirstOrDefaultAsync(b => b.BacSiId == bacSiId);
            if (bacSi == null) return Json(new { services = new List<object>(), bookedSlots = new List<string>() });

            // ĐIỀU KIỆN LẤY DỊCH VỤ THEO CHUYÊN KHOA CỦA BÁC SĨ
            var services = await _context.ChuyenKhoaDichVus
                .Where(ck => ck.ChuyenKhoaId == bacSi.ChuyenKhoaId && ck.DichVu.IsActive)
                .Select(ck => new {
                    value = ck.DichVu.DichVuId,
                    text = ck.DichVu.TenDichVu,
                    gia = ck.DichVu.Gia.ToString("N0")
                })
                .ToListAsync();

            var bookedSlots = await _context.LichHen
                .Where(lh => lh.BacSiId == bacSiId && lh.LichLamViec.NgayLamViec.Date == DateTime.Today && lh.TrangThai != TrangThaiLichHen.DaHuy)
                .Select(lh => lh.KhungGioBatDau.ToString(@"hh\:mm"))
                .ToListAsync();

            return Json(new { services, bookedSlots });
        }

        [HttpGet]
        public async Task<IActionResult> GetAppointmentSyncData(int id)
        {
            var lichHen = await _context.LichHen.Include(l => l.BacSi).FirstOrDefaultAsync(l => l.LichHenId == id);
            if (lichHen == null) return NotFound();
            var benhNhan = await _context.BenhNhan.FirstOrDefaultAsync(b => b.SoDienThoai == lichHen.SoDienThoai);
            return Json(new
            {
                benhNhanId = benhNhan?.BenhNhanId,
                bacSiId = lichHen.BacSiId,
                trieuChung = lichHen.TrieuChung,
                lichLamViecId = lichHen.LichLamViecId,
                khungGioStr = lichHen.KhungGioBatDau.ToString(@"hh\:mm")
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetPatientTimeline(int benhNhanId, int? currentHoSoId)
        {
            var history = await _context.HoSoBenhAn.Include(h => h.BacSi).Include(h => h.HinhAnhBenhAns)
                .Where(h => h.BenhNhanId == benhNhanId && h.Id != currentHoSoId)
                .OrderByDescending(h => h.NgayKham).ToListAsync();
            if (history == null || !history.Any()) return Content("");
            return PartialView("_PatientTimeline", history);
        }

        [HttpGet]
        public async Task<IActionResult> GetDoctorShiftData(int bacSiId)
        {
            var bacSi = await _context.BacSi.FirstOrDefaultAsync(b => b.BacSiId == bacSiId);
            if (bacSi == null) return Json(new { services = new List<object>(), bookedSlots = new List<string>(), shiftInfo = (object)null });

            // ĐIỀU KIỆN LẤY DỊCH VỤ THEO CHUYÊN KHOA CỦA BÁC SĨ (Giống LichHenController)
            var rawServices = await _context.ChuyenKhoaDichVus
                .Where(ck => ck.ChuyenKhoaId == bacSi.ChuyenKhoaId && ck.DichVu.IsActive)
                .Select(ck => new { ck.DichVu.DichVuId, ck.DichVu.TenDichVu, ck.DichVu.Gia })
                .ToListAsync();

            var services = rawServices.Select(s => new { value = s.DichVuId, text = s.TenDichVu, gia = s.Gia.ToString("N0") }).ToList();

            var shift = await _context.LichLamViec.FirstOrDefaultAsync(l => l.BacSiId == bacSiId && l.NgayLamViec.Date == DateTime.Today && l.IsActive);

            var bookedHen = await _context.LichHen.Include(lh => lh.LichLamViec)
                .Where(lh => lh.BacSiId == bacSiId && lh.LichLamViec.NgayLamViec.Date == DateTime.Today && lh.TrangThai != TrangThaiLichHen.DaHuy)
                .Select(lh => (TimeSpan?)lh.KhungGioBatDau).ToListAsync();

            var bookedHoSo = await _context.HoSoBenhAn.Where(h => h.BacSiId == bacSiId && h.NgayKham.Date == DateTime.Today)
                .Select(h => (TimeSpan?)h.KhungGioBatDau).ToListAsync();

            var allBookedSlots = bookedHen.Union(bookedHoSo).Where(t => t.HasValue).Select(t => t.Value.ToString(@"hh\:mm")).Distinct().ToList();

            return Json(new
            {
                services,
                bookedSlots = allBookedSlots,
                shiftInfo = shift != null ? new
                {
                    id = shift.LichLamViecId,
                    start = shift.GioBatDau.ToString(@"hh\:mm"),
                    end = shift.GioKetThuc.ToString(@"hh\:mm"),
                    duration = shift.ThoiLuongKhungGioPhut
                } : null
            });
        }

        // ===============================================================
        // 8. BỔ SUNG: GỢI Ý BỆNH LÝ & PHÁC ĐỒ
        // ===============================================================
        [HttpGet]
        public async Task<IActionResult> GetBenhLySuggestions(string term)
        {
            if (string.IsNullOrEmpty(term)) return Json(new List<object>());

            var results = await _context.BenhLy
                .Where(b => b.BenhLyId.Contains(term) || b.TenBenhLy.Contains(term))
                .Select(b => new {
                    id = b.BenhLyId,
                    text = "[" + b.BenhLyId + "] " + b.TenBenhLy
                })
                .Take(15)
                .ToListAsync();

            return Json(results);
        }

        [HttpGet]
        public async Task<IActionResult> GetPhacDoBenh(string id)
        {
            var benh = await _context.BenhLy
                .Where(b => b.BenhLyId == id)
                .Select(b => new {
                    ten = b.TenBenhLy,
                    phacDo = b.PhuongPhapDieuTri,
                    trieuChung = b.TrieuChung
                })
                .FirstOrDefaultAsync();

            if (benh == null) return NotFound();
            return Json(benh);
        }
    }
}