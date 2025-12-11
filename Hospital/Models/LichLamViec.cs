using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Hospital.Models
{
    public class LichLamViec
    {
        [Key]
        public int LichLamViecId { get; set; }

        // Ngày làm việc cụ thể
        [Required(ErrorMessage = "Vui lòng chọn Ngày làm việc.")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày làm việc")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime NgayLamViec { get; set; }

        // Giờ bắt đầu ca làm việc (Đã chuyển sang TimeSpan)
        [Required(ErrorMessage = "Vui lòng nhập Giờ bắt đầu.")]
        [Display(Name = "Giờ bắt đầu")]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = @"{0:hh\:mm}", ApplyFormatInEditMode = true)]
        public TimeSpan GioBatDau { get; set; }

        // Giờ kết thúc ca làm việc (Đã chuyển sang TimeSpan)
        [Required(ErrorMessage = "Vui lòng nhập Giờ kết thúc.")]
        [Display(Name = "Giờ kết thúc")]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = @"{0:hh\:mm}", ApplyFormatInEditMode = true)]
        public TimeSpan GioKetThuc { get; set; }

        // TRƯỜNG MỚI: Quy tắc phân chia khung giờ
        [Required(ErrorMessage = "Vui lòng nhập Thời lượng khung giờ.")]
        [Range(15, 180, ErrorMessage = "Thời lượng khung giờ phải từ 15 đến 180 phút.")]
        [Display(Name = "Thời lượng Khung giờ (Phút)")]
        public int ThoiLuongKhungGioPhut { get; set; } = 30; // Mặc định 30 phút

        // Khóa ngoại: Bác sĩ nào làm việc trong ca này
        [Required(ErrorMessage = "Vui lòng chọn Bác sĩ.")]
        [Display(Name = "Bác sĩ")]
        public int BacSiId { get; set; }
        [ForeignKey("BacSiId")]
        [ValidateNever]
        public BacSi BacSi { get; set; }

        // Trạng thái ca làm việc
        [Display(Name = "Trạng thái")]
        public bool IsActive { get; set; } = true;

        // Navigation Property: Lịch hẹn được đặt trong ca này
        [ValidateNever]
        public ICollection<LichHen> LichHens { get; set; }
    }
}