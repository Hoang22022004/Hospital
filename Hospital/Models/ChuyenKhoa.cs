using System.ComponentModel.DataAnnotations;
using System.Collections.Generic; // Cần thiết cho ICollection

namespace Hospital.Models
{
    public class ChuyenKhoa
    {
        [Key]
        public int ChuyenKhoaId { get; set; }

        [Required(ErrorMessage = "Tên chuyên khoa là bắt buộc.")]
        [StringLength(100)]
        [Display(Name = "Tên Chuyên khoa")]
        public string TenChuyenKhoa { get; set; } = string.Empty; // Khởi tạo để tránh null warnings

        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        // *******************************************************************
        // 1. Mối quan hệ 1-N với Bác sĩ (Giữ nguyên)
        // *******************************************************************
        public virtual ICollection<BacSi>? BacSis { get; set; }

        // *******************************************************************
        // 2. THÊM MỚI: Mối quan hệ N-N với Dịch vụ (Qua bảng trung gian)
        // *******************************************************************
        public virtual ICollection<ChuyenKhoaDichVu>? ChuyenKhoaDichVus { get; set; }
    }
}