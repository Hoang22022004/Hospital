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
        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime NgayLamViec { get; set; }

        // Giờ bắt đầu ca làm việc
        [Required]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime GioBatDau { get; set; }

        // Giờ kết thúc ca làm việc
        [Required]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime GioKetThuc { get; set; }

        // Khóa ngoại: Bác sĩ nào làm việc trong ca này
        [Required]
        public int BacSiId { get; set; }
        [ForeignKey("BacSiId")]
        [ValidateNever]
        public BacSi BacSi { get; set; }
        
        // Trạng thái ca làm việc (Ví dụ: Đã duyệt, Đã hủy)
        public bool IsActive { get; set; } = true;

        // Navigation Property: Lịch hẹn được đặt trong ca này
        [ValidateNever]
        public ICollection<LichHen> LichHens { get; set; }
    }
}