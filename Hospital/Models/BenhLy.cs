using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital.Models
{
    public class BenhLy
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required]
        [StringLength(10)]
        public string BenhLyId { get; set; } // Khóa chính không được null

        [Required(ErrorMessage = "Tên bệnh lý là bắt buộc.")]
        [StringLength(200)]
        public string TenBenhLy { get; set; } // Tên bệnh cũng thường bắt buộc

        // THÊM DẤU ? ĐỂ CHO PHÉP NULL TRONG C#
        public string? MoTaTongQuan { get; set; }
        public string? TrieuChung { get; set; }
        public string? PhuongPhapDieuTri { get; set; }
        public string? HinhAnhUrl { get; set; }

        public bool IsPublished { get; set; } = true;
        public DateTime NgayCapNhat { get; set; } = DateTime.Now;
    }
}