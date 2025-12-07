using System.ComponentModel.DataAnnotations;

namespace Hospital.Models
{
    public class BenhLy
    {
        // Khóa chính
        [Key]
        public int BenhLyId { get; set; }

        // Tên bệnh lý (Ví dụ: Mụn trứng cá, Viêm da cơ địa, Nám da)
        [Required(ErrorMessage = "Tên bệnh lý là bắt buộc.")]
        [StringLength(200)]
        public string TenBenhLy { get; set; }

        // Mô tả tổng quan về bệnh lý
        public string MoTaTongQuan { get; set; }

        // Triệu chứng nhận biết
        public string TrieuChung { get; set; }

        // Phương pháp điều trị chung
        public string PhuongPhapDieuTri { get; set; }

        // Đường dẫn ảnh minh họa bệnh lý
        public string HinhAnhUrl { get; set; }

        // *************************************************************
        // Mối quan hệ Navigation Property (One-to-Many)
        // Một Bệnh lý có thể có nhiều Phác đồ Điều trị liên quan
        // *************************************************************
        public virtual ICollection<PhacDoDieuTri> PhacDoDieuTris { get; set; }
    }
}