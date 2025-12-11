// Hospital/Models/ChuyenKhoaDichVu.cs
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital.Models
{
    // Bảng trung gian cho mối quan hệ Nhiều-Nhiều giữa Chuyên khoa và Dịch vụ
    public class ChuyenKhoaDichVu
    {
        // Khóa ngoại 1
        public int ChuyenKhoaId { get; set; }
        public ChuyenKhoa ChuyenKhoa { get; set; } // Navigation Property

        // Khóa ngoại 2
        public int DichVuId { get; set; }
        public DichVu DichVu { get; set; } // Navigation Property
    }
}