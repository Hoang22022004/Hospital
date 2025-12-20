using Hospital.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class ChiTietDichVu
{
    [Key]
    public int Id { get; set; }

    // Kết nối với Hồ sơ bệnh án
    public int HoSoBenhAnId { get; set; }
    [ForeignKey("HoSoBenhAnId")]
    public virtual HoSoBenhAn? HoSoBenhAn { get; set; }

    // Kết nối với bảng Dịch vụ bạn đã có
    public int DichVuId { get; set; }
    [ForeignKey("DichVuId")]
    public virtual DichVu? DichVu { get; set; }

    [Display(Name = "Số lượng")]
    public int SoLuong { get; set; } = 1;

    // Lưu đơn giá tại thời điểm khám (phòng trường hợp sau này giá dịch vụ thay đổi)
    public decimal DonGiaTaiThoiDiemKham { get; set; }
}