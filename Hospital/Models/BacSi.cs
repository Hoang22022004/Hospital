using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; // Cần thiết để dùng [ValidateNever]

namespace Hospital.Models
{
    public class BacSi
    {
        // Khóa chính
        [Key]
        public int BacSiId { get; set; }

        // Tên bác sĩ
        [Required(ErrorMessage = "Tên bác sĩ là bắt buộc.")]
        [StringLength(150)]
        [Display(Name = "Họ và Tên")]
        public string HoTen { get; set; }

        // Chuyên môn chính (Giữ lại nếu bạn cần mô tả thêm về chuyên môn)
        [Required(ErrorMessage = "Chuyên môn là bắt buộc.")]
        [StringLength(100)]
        public string ChuyenMon { get; set; }

        // Bằng cấp hoặc chức vụ
        [StringLength(100)]
        [Display(Name = "Bằng cấp/Chức vụ")]
        public string BangCap { get; set; }

        // Mô tả chi tiết
        [Display(Name = "Mô tả chi tiết")]
        public string MoTaChiTiet { get; set; }

        // Đường dẫn đến ảnh đại diện
        [Display(Name = "Ảnh đại diện")]
        public string? HinhAnhUrl { get; set; }

        // Trạng thái
        [Display(Name = "Trạng thái hoạt động")]
        public bool IsActive { get; set; } = true;

        // *************************************************************
        // MỐI QUAN HỆ MỚI: CHUYÊN KHOA (BẮT BUỘC)
        // *************************************************************

        [Required(ErrorMessage = "Chuyên khoa là bắt buộc.")]
        [Display(Name = "Chuyên khoa")]
        public int ChuyenKhoaId { get; set; } // Khóa ngoại

        [ForeignKey("ChuyenKhoaId")]
        [ValidateNever]
        public virtual ChuyenKhoa ChuyenKhoa { get; set; } = null!; // Navigation Property

        // *************************************************************
        // MỐI QUAN HỆ VỚI TÀI KHOẢN ĐĂNG NHẬP (ApplicationUser)
        // *************************************************************

        // Khóa ngoại liên kết với bảng AspNetUsers.
        [Required]
        public string IdentityUserId { get; set; }

        // Navigation Property: Tài khoản đăng nhập
        [ValidateNever]
        public virtual ApplicationUser User { get; set; }

        // *************************************************************
        // NAVIGATION PROPERTIES VỚI CÁC BẢNG KHÁC (Đã cấu hình trong DbContext)
        // *************************************************************

        // Một Bác sĩ có thể có nhiều Lịch làm việc
        [ValidateNever]
        public virtual ICollection<LichLamViec>? LichLamViecs { get; set; }
    }
}