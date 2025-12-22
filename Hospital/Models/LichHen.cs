using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

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
        [Required(ErrorMessage = "Vui lòng nhập Tên khách hàng.")]
        [StringLength(100)]
        [Display(Name = "Tên Khách hàng")]
        public string TenKhachHang { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Số điện thoại.")]
        [StringLength(15)]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; }

        [EmailAddress(ErrorMessage = "Địa chỉ Email không hợp lệ.")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        // *************************************************************
        // TRƯỜNG MỚI BỔ SUNG CHO LOGIC KHUNG GIỜ NHỎ
        // *************************************************************

        [Required(ErrorMessage = "Vui lòng chọn Khung giờ.")]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = @"{0:hh\:mm}", ApplyFormatInEditMode = true)]
        [Display(Name = "Khung giờ Bắt đầu")]
        public TimeSpan KhungGioBatDau { get; set; }

        // *************************************************************
        // Liên kết với các bảng khác
        // *************************************************************

        // Khóa ngoại: Ngày và Ca làm việc lớn
        [Required]
        [Display(Name = "Ca làm việc (Ngày)")]
        public int LichLamViecId { get; set; }
        [ForeignKey("LichLamViecId")]
        [ValidateNever]
        public virtual LichLamViec LichLamViec { get; set; }

        // Khóa ngoại: Bác sĩ được đặt hẹn
        [Required]
        [Display(Name = "Bác sĩ")]
        public int BacSiId { get; set; }
        [ForeignKey("BacSiId")]
        [ValidateNever]
        public virtual BacSi BacSi { get; set; }

        // Khóa ngoại: Dịch vụ được chọn
        [Required]
        [Display(Name = "Dịch vụ")]
        public int DichVuId { get; set; }
        [ForeignKey("DichVuId")]
        [ValidateNever]
        public virtual DichVu DichVu { get; set; }

        // Triệu chứng/Lý do khám bệnh
        [Display(Name = "Triệu chứng/Yêu cầu")]
        public string? TrieuChung { get; set; }

        // Ngày/Giờ đặt hẹn (Thời điểm tạo bản ghi)
        [Display(Name = "Thời gian Đặt")]
        public DateTime ThoiGianDat { get; set; } = DateTime.Now;

        // Trạng thái của lịch hẹn
        [Display(Name = "Trạng thái")]
        public TrangThaiLichHen TrangThai { get; set; } = TrangThaiLichHen.ChoDuyet;
        // Thêm vào trong class LichHen
        public virtual ICollection<HoSoBenhAn>? HoSoBenhAns { get; set; }
    }
}