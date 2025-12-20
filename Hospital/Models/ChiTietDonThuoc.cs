using Hospital.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class ChiTietDonThuoc
{
    [Key]
    public int Id { get; set; }

    // Kết nối với Hồ sơ bệnh án
    public int HoSoBenhAnId { get; set; }
    [ForeignKey("HoSoBenhAnId")]
    public virtual HoSoBenhAn? HoSoBenhAn { get; set; }

    // Kết nối với bảng Thuốc bạn đã có
    public int ThuocId { get; set; }
    [ForeignKey("ThuocId")]
    public virtual Thuoc? Thuoc { get; set; }

    [Display(Name = "Số lượng")]
    public int SoLuong { get; set; }

    [Display(Name = "Cách dùng")]
    public string? LieuDung { get; set; } // Ví dụ: "Sáng 1 viên, tối 1 viên sau ăn"
}