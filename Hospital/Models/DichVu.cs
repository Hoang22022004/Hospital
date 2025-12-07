using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital.Models
{
    public class DichVu
    {
        // Khóa chính
        [Key]
        public int DichVuId { get; set; }

        // Tên dịch vụ (Ví dụ: Trị mụn chuyên sâu, Laser Pico)
        [Required(ErrorMessage = "Tên dịch vụ là bắt buộc.")]
        [StringLength(200)]
        public string TenDichVu { get; set; }

        // Mô tả ngắn gọn để hiển thị trên trang chủ
        [StringLength(500)]
        public string MoTaNgan { get; set; }

        // Mô tả chi tiết cho trang chi tiết dịch vụ
        public string MoTaChiTiet { get; set; }

        // Giá dịch vụ (Sử dụng kiểu Decimal cho dữ liệu tiền tệ)
        [Required(ErrorMessage = "Giá dịch vụ là bắt buộc.")]
        [Column(TypeName = "decimal(18, 2)")] // Cấu hình kiểu tiền tệ trong CSDL
        public decimal Gia { get; set; }

        // Đường dẫn ảnh đại diện của dịch vụ
        public string AnhDichVuUrl { get; set; }

        // Trạng thái: Dịch vụ còn được cung cấp hay không
        public bool IsActive { get; set; } = true;

        // Thuộc tính quan trọng: Đánh dấu dịch vụ nổi bật trên Trang Chủ (Hot Service)
        [Display(Name = "Dịch vụ nổi bật")]
        public bool IsHot { get; set; } = false;

        // *************************************************************
        // Mối quan hệ Navigation Property (sẽ dùng khi tạo Model BenhLy)
        // Một Dịch vụ có thể điều trị nhiều Bệnh lý, hoặc ngược lại.
        // Tạm thời để trống.
        // public virtual ICollection<BenhLyDichVu> BenhLyDichVu { get; set; }
        // *************************************************************
    }
}