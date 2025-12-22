using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Hospital.Data;
using Hospital.Models;
using Microsoft.AspNetCore.Hosting;
using System.Linq;

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
                .Include(h => h.LichHen)
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
        // 3. TIẾP NHẬN BỆNH NHÂN (CREATE - NÂNG CẤP)
        // ==========================================
        public IActionResult Create(int? benhNhanId)
        {
            // === CHÈN CODE MỚI VÀO ĐÂY ===
            var todayAppointments = _context.LichHen
                .Include(l => l.BacSi)
                .Include(l => l.LichLamViec)
                .Where(l => l.LichLamViec.NgayLamViec.Date == DateTime.Today && l.TrangThai == TrangThaiLichHen.DaXacNhan)
                .Select(l => new { Id = l.LichHenId, Text = $"[{l.KhungGioBatDau:hh\\:mm}] {l.TenKhachHang} - BS.{l.BacSi.HoTen}" })
                .ToList();
            ViewBag.TodayAppointments = new SelectList(todayAppointments, "Id", "Text");
            // ============================

            ViewData["BenhNhanId"] = new SelectList(_context.BenhNhan, "BenhNhanId", "HoTen", benhNhanId);
            ViewData["BacSiId"] = new SelectList(_context.BacSi, "BacSiId", "HoTen");
            return View();
        }

        // Endpoint AJAX: Lấy dịch vụ và các khung giờ đã bị chiếm của bác sĩ
        [HttpGet]
        public async Task<IActionResult> GetDoctorAdmissionData(int bacSiId)
        {
            // 1. Lấy thông tin bác sĩ để biết chuyên khoa
            var bacSi = await _context.BacSi.FirstOrDefaultAsync(b => b.BacSiId == bacSiId);
            if (bacSi == null) return Json(new { services = new List<object>(), bookedSlots = new List<string>() });

            // 2. Lấy danh sách dịch vụ thuộc chuyên khoa của bác sĩ này
            var services = await _context.ChuyenKhoaDichVus
                .Where(ck => ck.ChuyenKhoaId == bacSi.ChuyenKhoaId)
                .Select(ck => new {
                    value = ck.DichVu.DichVuId,
                    text = ck.DichVu.TenDichVu,
                    gia = ck.DichVu.Gia.ToString("N0")
                })
                .ToListAsync();

            // 3. Lấy các khung giờ ĐÃ ĐƯỢC ĐẶT HẸN hôm nay của bác sĩ (để lễ tân tránh xếp trùng)
            var bookedSlots = await _context.LichHen
                .Where(lh => lh.BacSiId == bacSiId &&
                             lh.LichLamViec.NgayLamViec.Date == DateTime.Today &&
                             lh.TrangThai != TrangThaiLichHen.DaHuy)
                .Select(lh => lh.KhungGioBatDau.ToString(@"hh\:mm"))
                .ToListAsync();

            return Json(new { services, bookedSlots });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HoSoBenhAn hoSo, int DichVuId)
        {
            if (ModelState.IsValid)
            {
                // Sử dụng Transaction để đảm bảo: Hoặc lưu cả 2, hoặc không lưu gì cả (tránh lỗi dữ liệu)
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // 1. Lấy thông tin Bệnh nhân để có Tên/SĐT lưu sang Lịch hẹn
                        var benhNhan = await _context.BenhNhan.FindAsync(hoSo.BenhNhanId);

                        // 2. TỰ ĐỘNG TẠO LỊCH HẸN (Để đánh dấu giờ này ĐÃ BẬN)
                        var lichHenTuDong = new LichHen
                        {
                            TenKhachHang = benhNhan.HoTen,
                            SoDienThoai = benhNhan.SoDienThoai,
                            Email = benhNhan.Email,
                            BacSiId = hoSo.BacSiId,
                            LichLamViecId = hoSo.LichLamViecId ?? 0,
                            KhungGioBatDau = hoSo.KhungGioBatDau ?? TimeSpan.Zero,
                            DichVuId = DichVuId, // Bạn có thể sửa thành ID dịch vụ mặc định hoặc lấy từ Form
                            TrangThai = TrangThaiLichHen.DaXacNhan, // Đánh dấu Đã xác nhận để hiện màu đỏ (Bận)
                            ThoiGianDat = DateTime.Now,
                            TrieuChung = hoSo.TrieuChung
                        };

                        _context.LichHen.Add(lichHenTuDong);
                        await _context.SaveChangesAsync(); // Lưu để lấy ID lịch hẹn

                        // 3. CẬP NHẬT HỒ SƠ BỆNH ÁN
                        hoSo.LichHenId = lichHenTuDong.LichHenId; // Liên kết hồ sơ với lịch hẹn vừa tạo
                        hoSo.TrangThai = TrangThaiHoSo.ChoKham;
                        hoSo.NgayKham = DateTime.Now;

                        _context.Add(hoSo);
                        await _context.SaveChangesAsync();

                        // Hoàn tất giao dịch
                        await transaction.CommitAsync();

                        TempData["success"] = "Tiếp nhận thành công. Giờ khám đã được cập nhật bận trên hệ thống.";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();

                        // Dòng này sẽ lấy thông báo chi tiết nhất từ "ruột" của lỗi
                        var message = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

                        ModelState.AddModelError("", "Lỗi tại đây: " + message);
                    }
                }
            }

            // Nếu lỗi Form, nạp lại dữ liệu cho các Dropdown
            ViewData["BenhNhanId"] = new SelectList(_context.BenhNhan, "BenhNhanId", "HoTen", hoSo.BenhNhanId);
            ViewData["BacSiId"] = new SelectList(_context.BacSi, "BacSiId", "HoTen", hoSo.BacSiId);
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

            // 1. Khởi tạo Transaction để bảo vệ dữ liệu
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var record = await _context.HoSoBenhAn.FindAsync(id);
                    if (record == null) return NotFound();

                    // 2. Cập nhật thông tin lâm sàng
                    record.ChanDoan = hoSoUpdate.ChanDoan;
                    record.TrieuChung = hoSoUpdate.TrieuChung;
                    record.TinhTrangDa = hoSoUpdate.TinhTrangDa;
                    record.ViTriTonThuong = hoSoUpdate.ViTriTonThuong;
                    record.MucDo = hoSoUpdate.MucDo;
                    record.LoiDan = hoSoUpdate.LoiDan;
                    record.NgayTaiKham = hoSoUpdate.NgayTaiKham;
                    record.TrangThai = TrangThaiHoSo.ChoThanhToan;

                    // 3. Lưu Dịch vụ kỹ thuật
                    if (selectedDichVu != null)
                    {
                        foreach (var dvId in selectedDichVu)
                        {
                            _context.ChiTietDichVu.Add(new ChiTietDichVu { HoSoBenhAnId = id, DichVuId = dvId });
                        }
                    }

                    // 4. Lưu Đơn thuốc & Trừ tồn kho thực tế
                    if (selectedThuoc != null)
                    {
                        for (int i = 0; i < selectedThuoc.Length; i++)
                        {
                            int tId = selectedThuoc[i];
                            int qty = (soLuong != null && soLuong.Length > i) ? soLuong[i] : 1;
                            var tStock = await _context.Thuoc.FindAsync(tId);

                            if (tStock != null)
                            {
                                tStock.SoLuongTon -= qty; // Trừ kho
                            }

                            _context.ChiTietDonThuoc.Add(new ChiTietDonThuoc
                            {
                                HoSoBenhAnId = id,
                                ThuocId = tId,
                                SoLuong = qty,
                                LieuDung = (lieuDung != null && lieuDung.Length > i) ? lieuDung[i] : ""
                            });
                        }
                    }

                    // 5. Xử lý lưu File ảnh vào thư mục và ghi DB
                    string path = Path.Combine(_hostEnvironment.WebRootPath, @"images/da-lieu");
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                    if (anhChinh != null)
                    {
                        string fn = "Main_" + Guid.NewGuid() + Path.GetExtension(anhChinh.FileName);
                        using (var fs = new FileStream(Path.Combine(path, fn), FileMode.Create))
                        {
                            await anhChinh.CopyToAsync(fs);
                        }
                        _context.HinhAnhBenhAn.Add(new HinhAnhBenhAn { DuongDan = "/images/da-lieu/" + fn, LaAnhChinh = true, HoSoBenhAnId = id });
                    }

                    if (anhPhu != null)
                    {
                        foreach (var item in anhPhu)
                        {
                            string fn = "Sub_" + Guid.NewGuid() + Path.GetExtension(item.FileName);
                            using (var fs = new FileStream(Path.Combine(path, fn), FileMode.Create))
                            {
                                await item.CopyToAsync(fs);
                            }
                            _context.HinhAnhBenhAn.Add(new HinhAnhBenhAn { DuongDan = "/images/da-lieu/" + fn, LaAnhChinh = false, HoSoBenhAnId = id });
                        }
                    }

                    // 6. Lưu tất cả thay đổi
                    await _context.SaveChangesAsync();

                    // 7. Xác nhận hoàn tất Giao dịch
                    await transaction.CommitAsync();

                    TempData["success"] = "Hoàn tất khám bệnh thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // 8. Nếu có bất kỳ lỗi nào, hủy bỏ toàn bộ quá trình (không trừ kho, không lưu DB)
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "Lỗi khi lưu dữ liệu: " + ex.Message);

                    // Nạp lại danh sách để hiển thị lại View nếu lỗi
                    ViewBag.DichVuList = await _context.DichVu.ToListAsync();
                    ViewBag.ThuocList = await _context.Thuoc.ToListAsync();
                    return View(hoSoUpdate);
                }
            }
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
            // 1. Tìm hồ sơ bệnh án kèm theo thông tin Lịch hẹn liên kết
            var hoSo = await _context.HoSoBenhAn
                .Include(h => h.LichHen)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (hoSo != null)
            {
                // 2. Cập nhật trạng thái Hồ sơ bệnh án thành Hoàn thành
                hoSo.TrangThai = TrangThaiHoSo.HoanThanh;

                // 3. ĐỒNG BỘ: Nếu hồ sơ này có lịch hẹn đi kèm, cập nhật lịch hẹn đó luôn
                if (hoSo.LichHen != null)
                {
                    hoSo.LichHen.TrangThai = TrangThaiLichHen.HoanThanh;
                }

                await _context.SaveChangesAsync();
                TempData["success"] = "Thanh toán thành công. Hồ sơ và Lịch hẹn đã được đóng.";
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
        // ==========================================
        // 7. HỦY HỒ SƠ & HỦY LỊCH HẸN LIÊN KẾT
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyHoSo(int id)
        {
            // Tìm hồ sơ kèm theo lịch hẹn liên kết
            var hoSo = await _context.HoSoBenhAn
                .Include(h => h.LichHen)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (hoSo == null)
            {
                return NotFound();
            }

            try
            {
                // 1. Cập nhật trạng thái Hồ sơ (Nếu trong Enum TrangThaiHoSo của bạn chưa có DaHuy, hãy dùng một trạng thái phù hợp hoặc xóa)
                // Ở đây tôi giả định bạn dùng trạng thái để lưu vết
                hoSo.TrangThai = (TrangThaiHoSo)99; // Giả định 99 là trạng thái Đã Hủy, hoặc bạn có thể xóa record

                // 2. Cập nhật trạng thái Lịch hẹn sang Đã Hủy
                if (hoSo.LichHen != null)
                {
                    hoSo.LichHen.TrangThai = TrangThaiLichHen.DaHuy;
                }

                await _context.SaveChangesAsync();
                TempData["success"] = "Đã hủy hồ sơ và lịch hẹn thành công.";
            }
            catch (Exception ex)
            {
                TempData["error"] = "Lỗi khi hủy: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
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
        // 1. Action lấy dữ liệu lịch sử theo ID bệnh nhân
        [HttpGet]
        public async Task<IActionResult> GetPatientTimeline(int benhNhanId, int? currentHoSoId)
        {
            // Cần .Include(h => h.HinhAnhBenhAns) để EF tải dữ liệu ảnh từ bảng liên quan
            var history = await _context.HoSoBenhAn
                .Include(h => h.BacSi)
                .Include(h => h.HinhAnhBenhAns)
                .Where(h => h.BenhNhanId == benhNhanId && h.Id != currentHoSoId)
                .OrderByDescending(h => h.NgayKham)
                .ToListAsync();

            if (history == null || !history.Any())
            {
                return Content(""); // JS sẽ nhận Content trống và ẩn cột Timeline
            }

            return PartialView("_PatientTimeline", history);
        }

        // ===============================================================
        // CODE BỔ SUNG: LẤY DỮ LIỆU CA TRỰC VÀ KHUNG GIỜ CHI TIẾT
        // ===============================================================
        [HttpGet]
        public async Task<IActionResult> GetDoctorShiftData(int bacSiId)
        {
            var bacSi = await _context.BacSi.FirstOrDefaultAsync(b => b.BacSiId == bacSiId);
            if (bacSi == null) return Json(new { services = new List<object>(), bookedSlots = new List<string>(), shiftInfo = (object)null });

            // 1. Lấy dữ liệu dịch vụ thô từ SQL (Không ToString ở đây)
            var rawServices = await _context.ChuyenKhoaDichVus
                .Where(ck => ck.ChuyenKhoaId == bacSi.ChuyenKhoaId)
                .Select(ck => new {
                    ck.DichVu.DichVuId,
                    ck.DichVu.TenDichVu,
                    ck.DichVu.Gia
                }).ToListAsync();

            // Định dạng lại tiền tệ sau khi đã lấy dữ liệu về bộ nhớ
            var services = rawServices.Select(s => new {
                value = s.DichVuId,
                text = s.TenDichVu,
                gia = s.Gia.ToString("N0") // Ở đây sẽ không bị lỗi nữa
            }).ToList();

            // 2. Lấy thông tin ca làm việc
            var shift = await _context.LichLamViec
                .FirstOrDefaultAsync(l => l.BacSiId == bacSiId && l.NgayLamViec.Date == DateTime.Today && l.IsActive);

            // 3. Lấy giờ bận thô (Kiểu TimeSpan) từ cả 2 bảng
            // 3. Lấy giờ bận từ Lịch hẹn (Đảm bảo lấy về danh sách trước)
            var bookedHen = await _context.LichHen
                .Include(lh => lh.LichLamViec)
                .Where(lh => lh.BacSiId == bacSiId &&
                             lh.LichLamViec.NgayLamViec.Date == DateTime.Today &&
                             lh.TrangThai != TrangThaiLichHen.DaHuy)
                .Select(lh => (TimeSpan?)lh.KhungGioBatDau) // Ép kiểu về nullable tại đây
                .ToListAsync();

            // Lấy giờ bận từ Hồ sơ bệnh án
            var bookedHoSo = await _context.HoSoBenhAn
                .Where(h => h.BacSiId == bacSiId && h.NgayKham.Date == DateTime.Today)
                .Select(h => (TimeSpan?)h.KhungGioBatDau) // Ép kiểu về nullable tại đây
                .ToListAsync();

            // Gộp danh sách và xử lý định dạng chuỗi
            var allBookedSlots = bookedHen
                .Union(bookedHoSo) // Lúc này cả 2 đã cùng kiểu TimeSpan? nên sẽ hết lỗi
                .Where(t => t.HasValue) // Chỉ lấy các giá trị thực, bỏ qua null
                .Select(t => t.Value.ToString(@"hh\:mm")) // Định dạng Giờ:Phút
                .Distinct()
                .ToList();

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
    }
}