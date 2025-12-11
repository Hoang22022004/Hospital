// File: Hospital/Models/DichVu.cs

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic; // Cần thiết cho ICollection

namespace Hospital.Models
{
    public class DichVu
    {
        [Key]
        public int DichVuId { get; set; }

        [Required(ErrorMessage = "Tên dịch vụ là bắt buộc.")]
        [StringLength(200)]
        [Display(Name = "Tên Dịch vụ")]
        public string TenDichVu { get; set; } = string.Empty; // Khởi tạo để tránh null warnings

        [StringLength(500)]
        [Display(Name = "Mô tả Ngắn")]
        public string? MoTaNgan { get; set; }

        [Display(Name = "Mô tả Chi tiết")]
        public string? MoTaChiTiet { get; set; }

        [Required(ErrorMessage = "Giá dịch vụ là bắt buộc.")]
        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Giá (VNĐ)")]
        public decimal Gia { get; set; }

        [Display(Name = "Ảnh Đại diện")]
        public string? AnhDichVuUrl { get; set; }

        [Display(Name = "Trạng thái Hoạt động")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Dịch vụ Nổi bật")]
        public bool IsHot { get; set; } = false;

        // ********** NAVIGATION PROPERTY CHO MỐI QUAN HỆ NHIỀU-NHIỀU **********
        // Một Dịch vụ có nhiều liên kết đến Chuyên khoa (thông qua bảng trung gian)
        public virtual ICollection<ChuyenKhoaDichVu> ChuyenKhoaDichVus { get; set; } = new List<ChuyenKhoaDichVu>();
        // ********************************************************************
    }
}