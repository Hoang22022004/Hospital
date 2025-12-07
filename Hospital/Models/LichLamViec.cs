using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital.Models
{
    public class LichLamViec
    {
        // Khóa chính
        [Key]
        public int LichLamViecId { get; set; }

        // Khóa ngoại liên kết với Bác sĩ
        [Required]
        public int BacSiId { get; set; }

        // Navigation Property: Bác sĩ phụ trách
        [ForeignKey("BacSiId")]
        public virtual BacSi BacSi { get; set; }

        // Ngày làm việc
        [Required(ErrorMessage = "Ngày làm việc là bắt buộc.")]
        [DataType(DataType.Date)]
        public DateTime NgayLamViec { get; set; }

        // Khung giờ bắt đầu và kết thúc (cho ngày đó)
        [Required(ErrorMessage = "Giờ bắt đầu là bắt buộc.")]
        [DataType(DataType.Time)]
        public TimeSpan GioBatDau { get; set; }

        [Required(ErrorMessage = "Giờ kết thúc là bắt buộc.")]
        [DataType(DataType.Time)]
        public TimeSpan GioKetThuc { get; set; }

        // Thời gian cho mỗi cuộc hẹn (ví dụ: 30 phút, 45 phút)
        [Required]
        public int ThoiGianMoiLanKhamPhut { get; set; } = 30;

        // Trạng thái: Có thể là Ngày nghỉ (False) hoặc Ngày làm việc (True)
        public bool IsAvailable { get; set; } = true;

        // Navigation Property: Các lịch hẹn đã được đặt trong khung giờ này
        public virtual ICollection<LichHen> LichHens { get; set; }
    }
}