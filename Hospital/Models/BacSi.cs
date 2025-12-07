using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity; // Cần thiết để dùng IdentityUserId

namespace Hospital.Models
{
    public class BacSi
    {
        // Khóa chính
        [Key]
        public int BacSiId { get; set; }

        // Tên bác sĩ (Thường trùng với FullName trong ApplicationUser)
        [Required(ErrorMessage = "Tên bác sĩ là bắt buộc.")]
        [StringLength(150)]
        public string HoTen { get; set; }

        // Chuyên môn chính (Ví dụ: Da liễu Thẩm mỹ, Trị mụn)
        [Required(ErrorMessage = "Chuyên môn là bắt buộc.")]
        [StringLength(100)]
        public string ChuyenMon { get; set; }

        // Bằng cấp hoặc chức vụ
        [StringLength(100)]
        public string BangCap { get; set; }

        // Mô tả chi tiết về kinh nghiệm làm việc
        public string MoTaChiTiet { get; set; }

        // Đường dẫn đến ảnh đại diện của bác sĩ (Lưu dưới dạng chuỗi)
        public string HinhAnhUrl { get; set; }

        // Trạng thái: Dùng để hiển thị/ẩn bác sĩ trên trang Khách hàng
        public bool IsActive { get; set; } = true;

        // *************************************************************
        // Mối quan hệ với tài khoản đăng nhập (ApplicationUser)
        // *************************************************************

        // Khóa ngoại liên kết với bảng AspNetUsers. 
        // Đây là cột quan trọng để phân quyền cho giao diện Bác sĩ.
        [Required]
        [ForeignKey("ApplicationUser")] // Cần thiết lập rõ ràng
        public string IdentityUserId { get; set; }

        // Navigation Property: Tài khoản đăng nhập
        public virtual ApplicationUser User { get; set; }

        // Navigation Property: Các lịch hẹn được gán cho bác sĩ này (sẽ dùng khi tạo Model LichHen)
        // public virtual ICollection<LichHen> LichHens { get; set; }
    }
}