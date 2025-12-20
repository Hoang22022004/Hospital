using Hospital.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class HinhAnhBenhAn
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string DuongDan { get; set; } // Đường dẫn: /images/da-lieu/ten-file.jpg

    public bool LaAnhChinh { get; set; } // Phân biệt ảnh tổng quát (để so sánh) và ảnh chi tiết

    public string? GhiChu { get; set; } // Ví dụ: "Vùng má trái", "Cận cảnh nốt sưng"

    // Khóa ngoại kết nối với Hồ sơ bệnh án
    public int HoSoBenhAnId { get; set; }
    [ForeignKey("HoSoBenhAnId")]
    public virtual HoSoBenhAn? HoSoBenhAn { get; set; }
}