// Trong Hospital/Models/DichVu.cs

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital.Models
{
    public class DichVu
    {
        [Key]
        public int DichVuId { get; set; }

        [Required(ErrorMessage = "Tên dịch vụ là bắt buộc.")]
        [StringLength(200)]
        [Display(Name = "Tên Dịch vụ")] // <--- ĐÃ VIỆT HÓA
        public string TenDichVu { get; set; }

        [StringLength(500)]
        [Display(Name = "Mô tả Ngắn")] // <--- ĐÃ VIỆT HÓA
        public string? MoTaNgan { get; set; }

        [Display(Name = "Mô tả Chi tiết")] // <--- ĐÃ VIỆT HÓA
        public string? MoTaChiTiet { get; set; } // Đã thêm '?' cho phép NULL

        [Required(ErrorMessage = "Giá dịch vụ là bắt buộc.")]
        [Column(TypeName = "decimal(18, 2)")] 
        [Display(Name = "Giá (VNĐ)")] // <--- ĐÃ VIỆT HÓA
        public decimal Gia { get; set; }

        [Display(Name = "Ảnh Đại diện")] // <--- ĐÃ VIỆT HÓA
        public string? AnhDichVuUrl { get; set; } // Đã thêm '?' cho phép NULL

        [Display(Name = "Trạng thái Hoạt động")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Dịch vụ Nổi bật")]
        public bool IsHot { get; set; } = false;
    }
}