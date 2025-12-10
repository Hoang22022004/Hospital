using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; // Cần bổ sung này

namespace Hospital.Models
{
    // Enum cho Trạng thái Lịch hẹn (Giữ nguyên)
    public enum TrangThaiLichHen
    {
        ChoDuyet,
        DaXacNhan,
        DaHuy,
        HoanThanh
    }

    public class LichHen
    {
        // Khóa chính (Giữ nguyên)
        [Key]
        public int LichHenId { get; set; }

        // Thông tin Khách hàng (Giữ nguyên)
        [Required]
        [StringLength(100)]
        public string TenKhachHang { get; set; }

        [Required]
        [StringLength(15)]
        public string SoDienThoai { get; set; }

        [EmailAddress]
        public string? Email { get; set; } // Nên cho phép Email null nếu không bắt buộc

        // *************************************************************
        // Liên kết với các bảng khác
        // *************************************************************

        // Khóa ngoại: Ngày và Khung giờ hẹn cụ thể
        [Required]
        public int LichLamViecId { get; set; }
        [ForeignKey("LichLamViecId")]
        [ValidateNever] // 🛑 BỔ SUNG: Ngăn EF Core/Razor Validator kiểm tra
        public virtual LichLamViec LichLamViec { get; set; }

        // Khóa ngoại: Bác sĩ được đặt hẹn
        [Required]
        public int BacSiId { get; set; }
        [ForeignKey("BacSiId")]
        [ValidateNever] // 🛑 BỔ SUNG
        public virtual BacSi BacSi { get; set; }

        // Khóa ngoại: Dịch vụ được chọn
        [Required]
        public int DichVuId { get; set; }
        [ForeignKey("DichVuId")]
        [ValidateNever] // 🛑 BỔ SUNG
        public virtual DichVu DichVu { get; set; }

        // Triệu chứng/Lý do khám bệnh
        public string? TrieuChung { get; set; } // Cho phép null

        // Ngày/Giờ đặt hẹn (Thời điểm tạo bản ghi)
        public DateTime ThoiGianDat { get; set; } = DateTime.Now;

        // Trạng thái của lịch hẹn
        public TrangThaiLichHen TrangThai { get; set; } = TrangThaiLichHen.ChoDuyet;
    }
}