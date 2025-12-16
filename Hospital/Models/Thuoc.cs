using System.ComponentModel.DataAnnotations;

namespace Hospital.Models
{
    // 1. Định nghĩa danh sách cố định (Enum)
    public enum PhanLoaiThuoc
    {
        [Display(Name = "Thuốc uống")]
        ThuocUong = 0,

        [Display(Name = "Thuốc bôi ngoài da")]
        ThuocBoi = 1,

        [Display(Name = "Dược mỹ phẩm (Sữa rửa mặt/Kem dưỡng)")]
        DuocMyPham = 2,

        [Display(Name = "Vật tư y tế")]
        VatTu = 3
    }

    public class Thuoc
    {
        [Key]
        public int ThuocId { get; set; }

        [Required(ErrorMessage = "Tên thuốc là bắt buộc")]
        [Display(Name = "Tên thuốc / Sản phẩm")]
        public string TenThuoc { get; set; }

        // --- BỔ SUNG TRƯỜNG THƯƠNG HIỆU ---
        [Display(Name = "Thương hiệu")]
        public string? ThuongHieu { get; set; }
        // ----------------------------------

        [Display(Name = "Hoạt chất")]
        public string? HoatChat { get; set; }

        [Display(Name = "Công dụng / Chỉ định")]
        public string? CongDung { get; set; } 

        [Required]
        [Display(Name = "Đơn vị tính")]
        public string DonViTinh { get; set; } // Giữ nguyên là string để linh động

        [Display(Name = "Giá nhập")]
        public decimal GiaNhap { get; set; }

        [Required]
        [Display(Name = "Giá bán")]
        public decimal GiaBan { get; set; }

        [Display(Name = "Tồn kho")]
        public int SoLuongTon { get; set; } = 0;

        [Display(Name = "Hạn sử dụng")]
        [DataType(DataType.Date)]
        public DateTime? HanSuDung { get; set; }

        [Display(Name = "Hình ảnh")]
        public string? HinhAnhUrl { get; set; }

        [Display(Name = "Cách dùng")]
        public string? CachDung { get; set; }

        public bool IsActive { get; set; } = true;

        // 2. Sử dụng Enum thay vì liên kết bảng
        [Display(Name = "Phân loại")]
        [Required(ErrorMessage = "Vui lòng chọn loại thuốc")]
        public PhanLoaiThuoc PhanLoai { get; set; }
    }
}