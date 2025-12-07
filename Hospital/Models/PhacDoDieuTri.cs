using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital.Models
{
    public class PhacDoDieuTri
    {
        // Khóa chính
        [Key]
        public int PhacDoId { get; set; }

        // *************************************************************
        // Khóa ngoại liên kết với bảng BenhLy (One-to-Many)
        // *************************************************************
        [Required]
        public int BenhLyId { get; set; }

        // Navigation Property: Bệnh lý liên quan
        [ForeignKey("BenhLyId")]
        public virtual BenhLy BenhLy { get; set; }

        // Tên Thuốc/Phương pháp (Ví dụ: Isotretinoin, Peel da hóa học)
        [Required(ErrorMessage = "Tên thuốc/phương pháp là bắt buộc.")]
        public string TenThuocPhuongPhap { get; set; }

        // Liều lượng hoặc tần suất sử dụng
        public string LieuLuong { get; set; }

        // Tác dụng phụ thường gặp
        public string TacDungPhu { get; set; }

        // Ghi chú hoặc lời khuyên của chuyên gia
        public string GhiChu { get; set; }
    }
}