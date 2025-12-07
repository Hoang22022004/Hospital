using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital.Models
{
    // Enum cho Trạng thái Lịch hẹn
    public enum TrangThaiLichHen
    {
        ChoDuyet,       // Khách hàng mới đặt
        DaXacNhan,      // Admin/Bác sĩ xác nhận
        DaHuy,          // Khách hàng/Admin hủy
        HoanThanh       // Đã khám xong
    }

    public class LichHen
    {
        // Khóa chính
        [Key]
        public int LichHenId { get; set; }

        // Thông tin Khách hàng (Nếu không đăng nhập, dùng thông tin này)
        [Required]
        [StringLength(100)]
        public string TenKhachHang { get; set; }

        [Required]
        [StringLength(15)]
        public string SoDienThoai { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        // *************************************************************
        // Liên kết với các bảng khác
        // *************************************************************

        // Khóa ngoại: Ngày và Khung giờ hẹn cụ thể
        [Required]
        public int LichLamViecId { get; set; }
        [ForeignKey("LichLamViecId")]
        public virtual LichLamViec LichLamViec { get; set; }

        // Khóa ngoại: Bác sĩ được đặt hẹn
        [Required]
        public int BacSiId { get; set; }
        [ForeignKey("BacSiId")]
        public virtual BacSi BacSi { get; set; }

        // Khóa ngoại: Dịch vụ được chọn
        [Required]
        public int DichVuId { get; set; }
        [ForeignKey("DichVuId")]
        public virtual DichVu DichVu { get; set; }

        // Triệu chứng/Lý do khám bệnh
        public string TrieuChung { get; set; }

        // Ngày/Giờ đặt hẹn (Thời điểm tạo bản ghi)
        public DateTime ThoiGianDat { get; set; } = DateTime.Now;

        // Trạng thái của lịch hẹn
        public TrangThaiLichHen TrangThai { get; set; } = TrangThaiLichHen.ChoDuyet;
    }
}