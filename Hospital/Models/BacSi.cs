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
        public string HoTen { get; set; }

        // Chuyên môn chính
        [Required(ErrorMessage = "Chuyên môn là bắt buộc.")]
        [StringLength(100)]
        public string ChuyenMon { get; set; }

        // Bằng cấp hoặc chức vụ
        [StringLength(100)]
        public string BangCap { get; set; }

        // Mô tả chi tiết
        public string MoTaChiTiet { get; set; }

        // Đường dẫn đến ảnh đại diện (Đã là string? - Cho phép null)
        public string? HinhAnhUrl { get; set; }

        // Trạng thái
        public bool IsActive { get; set; } = true;

        // *************************************************************
        // Mối quan hệ với tài khoản đăng nhập (ApplicationUser)
        // *************************************************************

        // Khóa ngoại liên kết với bảng AspNetUsers.
        [Required]
        [ForeignKey("ApplicationUser")]
        public string IdentityUserId { get; set; }

        // Navigation Property: Tài khoản đăng nhập
        // Dùng [ValidateNever] để ngăn Model Binder cố gắng điền và validate đối tượng User
        [ValidateNever] // <--- DÒNG BỔ SUNG QUAN TRỌNG
        public virtual ApplicationUser User { get; set; }
    }
}