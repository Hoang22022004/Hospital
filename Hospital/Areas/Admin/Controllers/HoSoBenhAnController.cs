using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Hospital.Data;
using Hospital.Models;
using Microsoft.AspNetCore.Hosting;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Hospital.Helpers;
using Microsoft.Extensions.Options;

namespace Hospital.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Receptionist,Doctor,Customer")] // Cho phép cả 3 vào xem danh sách chung
    public class HoSoBenhAnController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IConfiguration _configuration; // <--- THÊM DÒNG NÀY
        private readonly VnpayConfig _vnpayConfig;
        public HoSoBenhAnController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment, IConfiguration configuration, IOptions<VnpayConfig> vnpayOptions) // <--- THÊM THAM SỐ NÀY
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
            _configuration = configuration; // <--- THÊM DÒNG NÀY
            _vnpayConfig = vnpayOptions.Value;
        }

        // --- HÀM HELPER: Lấy ID Bác sĩ từ tài khoản đang đăng nhập ---
        private async Task<BacSi?> GetCurrentBacSi()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return await _context.BacSi.FirstOrDefaultAsync(u => u.IdentityUserId == userId);
        }

        // ===============================================================
        // 1. DANH SÁCH HỒ SƠ & THỐNG KÊ (PHÂN QUYỀN LỌC DỮ LIỆU)
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

            // --- PHÂN QUYỀN LỌC DỮ LIỆU ---
            // Nếu là Doctor: Chỉ thấy hồ sơ của chính mình
            if (User.IsInRole("Doctor"))
            {
                var currentDoc = await GetCurrentBacSi();
                if (currentDoc != null)
                {
                    query = query.Where(h => h.BacSiId == currentDoc.BacSiId);
                }
            }
            // Admin và Receptionist mặc định thấy toàn bộ

            // Lọc theo ngày khám đang chọn
            query = query.Where(h => h.NgayKham.Date == searchDate.Value.Date);

            // --- THỐNG KÊ SỐ LƯỢNG (Dashboard Stats - Đã áp dụng lọc theo quyền) ---
            ViewBag.CountChoKham = await query.CountAsync(x => x.TrangThai == TrangThaiHoSo.ChoKham);
            ViewBag.CountDangKham = await query.CountAsync(x => x.TrangThai == TrangThaiHoSo.DangKham);
            ViewBag.CountChoThanhToan = await query.CountAsync(x => x.TrangThai == TrangThaiHoSo.ChoThanhToan);
            ViewBag.CountHoanThanh = await query.CountAsync(x => x.TrangThai == TrangThaiHoSo.HoanThanh);

            // --- BỘ LỌC TÌM KIẾM ---
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

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.SearchDate = searchDate.Value.ToString("yyyy-MM-dd");
            ViewBag.SearchString = searchString;
            ViewBag.Status = status;

            return View(records);
        }

        // ===============================================================
        // 2. CHI TIẾT BỆNH ÁN & TÍNH TIỀN (TẤT CẢ QUYỀN XEM ĐƯỢC)
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

            // Tính toán tổng tiền cho View
            decimal tongDichVu = hoSo.ChiTietDichVus?.Sum(d => d.DichVu.Gia) ?? 0;
            decimal tongThuoc = hoSo.ChiTietDonThuocs?.Sum(t => t.SoLuong * t.Thuoc.GiaBan) ?? 0;

            ViewBag.TongDichVu = (decimal)tongDichVu;
            ViewBag.TongThuoc = (decimal)tongThuoc;
            ViewBag.TongTien = (decimal)(tongDichVu + tongThuoc);

            return View(hoSo);
        }

        // ===============================================================
        // 3. TIẾP NHẬN BỆNH NHÂN (CHỈ ADMIN & RECEPTIONIST)
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
        // 4. BÁC SĨ KHÁM BỆNH (CHỈ DOCTOR)
        // ===============================================================
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> KhamBenh(int? id)
        {
            if (id == null) return NotFound();
            var hoSo = await _context.HoSoBenhAn.Include(h => h.BenhNhan).FirstOrDefaultAsync(m => m.Id == id);
            if (hoSo == null) return NotFound();

            // KIỂM TRA BẢO MẬT: Bác sĩ không được khám bệnh án của đồng nghiệp
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
            ViewBag.DichVuList = await _context.DichVu.ToListAsync();
            ViewBag.ThuocList = await _context.Thuoc.ToListAsync();
            return View(hoSo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> HoanTatKham(int id, HoSoBenhAn hoSoUpdate,
            IFormFile? anhChinh, List<IFormFile>? anhPhu,
            int[] selectedDichVu, int[] selectedThuoc, int[] soLuong, string[] lieuDung)
        {
            if (id != hoSoUpdate.Id) return NotFound();

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var record = await _context.HoSoBenhAn.FindAsync(id);
                    if (record == null) return NotFound();

                    // Cập nhật lâm sàng
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
                        foreach (var dvId in selectedDichVu)
                            _context.ChiTietDichVu.Add(new ChiTietDichVu { HoSoBenhAnId = id, DichVuId = dvId });

                    // Lưu Đơn thuốc & Trừ tồn kho
                    if (selectedThuoc != null)
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

                    // Lưu File ảnh
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
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    TempData["error"] = "Lỗi khi lưu dữ liệu!";
                    return RedirectToAction("KhamBenh", new { id = id });
                }
            }
        }

        // ===============================================================
        // 5. THANH TOÁN (CHỈ ADMIN & RECEPTIONIST)
        // ===============================================================
        //[Authorize(Roles = "Admin,Receptionist,Customer")]
        //public async Task<IActionResult> ThanhToan(int id)
        //{
        //    return await Details(id);
        //}
        //// ===============================================================
        //// CHIẾN LƯỢC THANH TOÁN VNPAY
        //// ===============================================================
        //[Authorize(Roles = "Admin,Receptionist,Customer")]
        //public async Task<IActionResult> ThanhToanVnpay(int id)
        //{
        //    var hoSo = await _context.HoSoBenhAn
        //        .Include(h => h.ChiTietDichVus).ThenInclude(d => d.DichVu)
        //        .Include(h => h.ChiTietDonThuocs).ThenInclude(t => t.Thuoc)
        //        .FirstOrDefaultAsync(h => h.Id == id);

        //    if (hoSo == null) return NotFound();

        //    // Bảo mật cho khách hàng
        //    if (User.IsInRole("Customer"))
        //    {
        //        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //        var benhNhan = await _context.BenhNhan.FirstOrDefaultAsync(b => b.IdentityUserId == userId);
        //        if (hoSo.BenhNhanId != benhNhan?.BenhNhanId) return Forbid();
        //    }

        //    decimal total = (hoSo.ChiTietDichVus?.Sum(d => d.DichVu.Gia) ?? 0) +
        //                    (hoSo.ChiTietDonThuocs?.Sum(t => t.SoLuong * t.Thuoc.GiaBan) ?? 0);

        //    var vnpay = new VnPayLibrary();
        //    vnpay.AddRequestData("vnp_Version", "2.1.0");
        //    vnpay.AddRequestData("vnp_Command", "pay");
        //    vnpay.AddRequestData("vnp_TmnCode", _vnpayConfig.TmnCode);
        //    vnpay.AddRequestData("vnp_Amount", ((long)(total * 100)).ToString());
        //    vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
        //    vnpay.AddRequestData("vnp_CurrCode", "VND");
        //    vnpay.AddRequestData("vnp_IpAddr", vnpay.GetIpAddress(HttpContext));
        //    vnpay.AddRequestData("vnp_Locale", "vn");
        //    vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan ho so benh an: " + id);
        //    vnpay.AddRequestData("vnp_OrderType", "other");
        //    vnpay.AddRequestData("vnp_ReturnUrl", _vnpayConfig.ReturnUrl);
        //    vnpay.AddRequestData("vnp_TxnRef", id.ToString() + "_" + DateTime.Now.Ticks);

        //    string paymentUrl = vnpay.CreateRequestUrl(_vnpayConfig.BaseUrl, _vnpayConfig.HashSecret);
        //    return Redirect(paymentUrl);
        //}
        //[HttpGet]
        //[AllowAnonymous] // Cho phép VNPay gọi về mà không bị chặn bởi phân quyền nếu cần
        //public async Task<IActionResult> VnpayReturn()
        //{
        //    var vnpayData = Request.Query;
        //    var vnpay = new VnPayLibrary();

        //    // 1. Lấy toàn bộ dữ liệu trả về, LOẠI BỎ vnp_SecureHash ra khỏi danh sách băm
        //    foreach (var (key, value) in vnpayData)
        //    {
        //        if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_") && key != "vnp_SecureHash")
        //        {
        //            vnpay.AddResponseData(key, value);
        //        }
        //    }

        //    // 2. Lấy các thông tin cần thiết
        //    string txnRef = vnpay.GetResponseData("vnp_TxnRef");
        //    string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
        //    string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
        //    string vnp_SecureHash = Request.Query["vnp_SecureHash"];

        //    // Kiểm tra an toàn tham chiếu giao dịch
        //    if (string.IsNullOrEmpty(txnRef) || !txnRef.Contains("_"))
        //        return BadRequest("Tham số giao dịch không hợp lệ.");

        //    int hoSoId = int.Parse(txnRef.Split('_')[0]);

        //    // 3. Kiểm tra chữ ký bảo mật (Validate Signature)
        //    bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, _vnpayConfig.HashSecret);

        //    if (checkSignature)
        //    {
        //        // Chữ ký đúng, kiểm tra tiếp mã phản hồi giao dịch (00 = Thành công)
        //        if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
        //        {
        //            var hoSo = await _context.HoSoBenhAn
        //                .Include(h => h.LichHen)
        //                .FirstOrDefaultAsync(h => h.Id == hoSoId);

        //            if (hoSo != null)
        //            {
        //                hoSo.TrangThai = TrangThaiHoSo.HoanThanh;
        //                if (hoSo.LichHen != null)
        //                    hoSo.LichHen.TrangThai = TrangThaiLichHen.HoanThanh;

        //                await _context.SaveChangesAsync();
        //                TempData["success"] = "Thanh toán hồ sơ bệnh án thành công!";
        //            }
        //        }
        //        else
        //        {
        //            // Lỗi từ phía ngân hàng hoặc người dùng hủy giao dịch
        //            TempData["error"] = $"Giao dịch thất bại. Mã lỗi ngân hàng: {vnp_ResponseCode}";
        //        }
        //    }
        //    else
        //    {
        //        // Lỗi bảo mật: Chữ ký không khớp (Có thể do sai HashSecret hoặc dữ liệu bị can thiệp)
        //        TempData["error"] = "Có lỗi xảy ra trong quá trình xác thực chữ ký bảo mật.";
        //    }

        //    // 4. Quay lại trang chi tiết hồ sơ
        //    return RedirectToAction("Details", new { id = hoSoId });
        //}
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

        // ===============================================================
        // 6. XÓA & HỦY HỒ SƠ (CHỈ ADMIN & RECEPTIONIST)
        // ===============================================================
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

            hoSo.TrangThai = (TrangThaiHoSo)99; // Trạng thái hủy
            if (hoSo.LichHen != null) hoSo.LichHen.TrangThai = TrangThaiLichHen.DaHuy;

            await _context.SaveChangesAsync();
            TempData["success"] = "Đã hủy hồ sơ thành công.";
            return RedirectToAction(nameof(Index));
        }

        // ===============================================================
        // 7. CÁC HÀM AJAX ĐỒNG BỘ DỮ LIỆU (TẤT CẢ GIỮ NGUYÊN CODE)
        // ===============================================================
        [HttpGet]
        public async Task<IActionResult> GetDoctorAdmissionData(int bacSiId)
        {
            var bacSi = await _context.BacSi.FirstOrDefaultAsync(b => b.BacSiId == bacSiId);
            if (bacSi == null) return Json(new { services = new List<object>(), bookedSlots = new List<string>() });

            var services = await _context.ChuyenKhoaDichVus
                .Where(ck => ck.ChuyenKhoaId == bacSi.ChuyenKhoaId)
                .Select(ck => new { value = ck.DichVu.DichVuId, text = ck.DichVu.TenDichVu, gia = ck.DichVu.Gia.ToString("N0") })
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
            return Json(new { benhNhanId = benhNhan?.BenhNhanId, bacSiId = lichHen.BacSiId, trieuChung = lichHen.TrieuChung, lichLamViecId = lichHen.LichLamViecId, khungGioStr = lichHen.KhungGioBatDau.ToString(@"hh\:mm") });
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

            var rawServices = await _context.ChuyenKhoaDichVus.Where(ck => ck.ChuyenKhoaId == bacSi.ChuyenKhoaId)
                .Select(ck => new { ck.DichVu.DichVuId, ck.DichVu.TenDichVu, ck.DichVu.Gia }).ToListAsync();

            var services = rawServices.Select(s => new { value = s.DichVuId, text = s.TenDichVu, gia = s.Gia.ToString("N0") }).ToList();

            var shift = await _context.LichLamViec.FirstOrDefaultAsync(l => l.BacSiId == bacSiId && l.NgayLamViec.Date == DateTime.Today && l.IsActive);

            var bookedHen = await _context.LichHen.Include(lh => lh.LichLamViec)
                .Where(lh => lh.BacSiId == bacSiId && lh.LichLamViec.NgayLamViec.Date == DateTime.Today && lh.TrangThai != TrangThaiLichHen.DaHuy)
                .Select(lh => (TimeSpan?)lh.KhungGioBatDau).ToListAsync();

            var bookedHoSo = await _context.HoSoBenhAn.Where(h => h.BacSiId == bacSiId && h.NgayKham.Date == DateTime.Today)
                .Select(h => (TimeSpan?)h.KhungGioBatDau).ToListAsync();

            var allBookedSlots = bookedHen.Union(bookedHoSo).Where(t => t.HasValue).Select(t => t.Value.ToString(@"hh\:mm")).Distinct().ToList();

            return Json(new { services, bookedSlots = allBookedSlots, shiftInfo = shift != null ? new { id = shift.LichLamViecId, start = shift.GioBatDau.ToString(@"hh\:mm"), end = shift.GioKetThuc.ToString(@"hh\:mm"), duration = shift.ThoiLuongKhungGioPhut } : null });
        }
        // ... (Các hàm AJAX cũ của bạn: GetPatientTimeline, GetDoctorShiftData...)

        // ===============================================================
        // 8. BỔ SUNG: GỢI Ý BỆNH LÝ & PHÁC ĐỒ (DÁN VÀO ĐÂY)
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
    } // Dấu đóng ngoặc cuối cùng của Class HoSoBenhAnController
} // Dấu đóng ngoặc cuối cùng của Namespace
 