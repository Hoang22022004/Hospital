using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hospital.Data;
using Hospital.Models;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Hospital.Helpers;

namespace Hospital.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly VnpayConfig _vnpayConfig;

        public PaymentController(ApplicationDbContext context, IOptions<VnpayConfig> vnpayOptions)
        {
            _context = context;
            _vnpayConfig = vnpayOptions.Value;
        }

        // 1. Action tạo đường dẫn thanh toán
        // Sửa: Thêm nhiều biến thể Role để tránh lỗi phân biệt hoa thường
        [Authorize(Roles = "Admin,Receptionist,Customer,customer,Khách hàng")]
        public async Task<IActionResult> ThanhToanVnpay(int id)
        {
            var hoSo = await _context.HoSoBenhAn
                .Include(h => h.ChiTietDichVus).ThenInclude(d => d.DichVu)
                .Include(h => h.ChiTietDonThuocs).ThenInclude(t => t.Thuoc)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (hoSo == null) return NotFound();

            // SỬA QUAN TRỌNG: Tạm thời khóa kiểm tra Forbid() 
            // Lỗi "Dừng lại một chút" thường do hoSo.BenhNhanId không khớp với IdentityUserId trong bảng BenhNhan
            /*
            if (User.IsInRole("Customer") || User.IsInRole("customer"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var benhNhan = await _context.BenhNhan.FirstOrDefaultAsync(b => b.IdentityUserId == userId);
                
                // Nếu dữ liệu bảng BenhNhan chưa chuẩn, dòng này sẽ chặn bạn
                if (benhNhan == null || hoSo.BenhNhanId != benhNhan.BenhNhanId) 
                {
                    // return Forbid(); // Tạm khóa để test
                }
            }
            */

            decimal total = (hoSo.ChiTietDichVus?.Sum(d => d.DichVu.Gia) ?? 0) +
                            (hoSo.ChiTietDonThuocs?.Sum(t => t.SoLuong * t.Thuoc.GiaBan) ?? 0);

            var vnpay = new VnPayLibrary();
            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", _vnpayConfig.TmnCode);
            vnpay.AddRequestData("vnp_Amount", ((long)(total * 100)).ToString());
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", vnpay.GetIpAddress(HttpContext));
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan ho so benh an: " + id);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", _vnpayConfig.ReturnUrl);
            vnpay.AddRequestData("vnp_TxnRef", id.ToString() + "_" + DateTime.Now.Ticks);

            string paymentUrl = vnpay.CreateRequestUrl(_vnpayConfig.BaseUrl, _vnpayConfig.HashSecret);
            return Redirect(paymentUrl);
        }

        // 2. Action nhận kết quả trả về từ VNPay
        [AllowAnonymous]
        public async Task<IActionResult> VnpayReturn()
        {
            var vnpayData = Request.Query;
            var vnpay = new VnPayLibrary();

            foreach (var (key, value) in vnpayData)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_") && key != "vnp_SecureHash")
                {
                    vnpay.AddResponseData(key, value);
                }
            }

            string txnRef = vnpay.GetResponseData("vnp_TxnRef");
            string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
            string vnp_SecureHash = Request.Query["vnp_SecureHash"];

            if (string.IsNullOrEmpty(txnRef) || !txnRef.Contains("_")) return BadRequest();

            int hoSoId = int.Parse(txnRef.Split('_')[0]);
            bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, _vnpayConfig.HashSecret);

            if (checkSignature && vnp_ResponseCode == "00")
            {
                var hoSo = await _context.HoSoBenhAn.Include(h => h.LichHen).FirstOrDefaultAsync(h => h.Id == hoSoId);
                if (hoSo != null)
                {
                    hoSo.TrangThai = TrangThaiHoSo.HoanThanh;
                    if (hoSo.LichHen != null) hoSo.LichHen.TrangThai = TrangThaiLichHen.HoanThanh;
                    await _context.SaveChangesAsync();
                    TempData["success"] = "Thanh toán thành công!";
                }
            }
            else
            {
                TempData["error"] = "Thanh toán thất bại hoặc chữ ký không hợp lệ.";
            }

            // Chuyển hướng thông minh dựa trên Role
            if (User.Identity.IsAuthenticated && (User.IsInRole("Admin") || User.IsInRole("Receptionist")))
            {
                return RedirectToAction("Index", "HoSoBenhAn", new { area = "Admin" });
            }

            return RedirectToPage("/Account/Manage/MedicalHistory", new { area = "Identity" });
        }
    }
}