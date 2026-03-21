using Hospital.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class ChiTietDonThuoc
{
    [Key]
    public int Id { get; set; }

    public int HoSoBenhAnId { get; set; }
    [ForeignKey("HoSoBenhAnId")]
    public virtual HoSoBenhAn? HoSoBenhAn { get; set; }

    public int ThuocId { get; set; }
    [ForeignKey("ThuocId")]
    public virtual Thuoc? Thuoc { get; set; }

    // Số lượng tổng cộng để xuất kho (Hệ thống sẽ tự tính: Tổng liều các buổi * Số ngày)
    [Display(Name = "Tổng số lượng")]
    public double SoLuong { get; set; }

    // Tách liều dùng theo từng buổi (Dùng kiểu string để lưu được "1", "1/2", "1/4")
    [Display(Name = "Sáng")]
    public string? LieuSang { get; set; }

    [Display(Name = "Trưa")]
    public string? LieuTrua { get; set; }

    [Display(Name = "Chiều")]
    public string? LieuChieu { get; set; }

    [Display(Name = "Tối")]
    public string? LieuToi { get; set; }

    // Số ngày sử dụng thuốc
    [Display(Name = "Số ngày dùng")]
    public int SoNgayDung { get; set; } = 1; // Mặc định là 1 ngày

    // Ghi chú cụ thể cho từng loại thuốc
    [Display(Name = "Ghi chú")]
    public string? GhiChu { get; set; }

    // Giữ lại cột này nếu bạn muốn lưu một chuỗi tổng hợp để in nhanh (Tùy chọn)
    public string? CachDungTongHop { get; set; }
}