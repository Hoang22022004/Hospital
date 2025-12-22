using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital.Models // Thay đổi theo namespace của bạn
{
    // 1. Định nghĩa Enum ngay tại đây để quản lý trạng thái hồ sơ
    public enum TrangThaiHoSo
    {
        [Display(Name = "Chờ khám")]
        ChoKham = 0,         // Lễ tân tạo, chờ bác sĩ gọi

        [Display(Name = "Đang khám")]
        DangKham = 1,        // Bác sĩ đang thực hiện khám

        [Display(Name = "Chờ thanh toán")]
        ChoThanhToan = 2,    // Đã khám xong, chờ ra quầy thu ngân

        [Display(Name = "Hoàn thành")]
        HoanThanh = 3,       // Đã đóng tiền và nhận thuốc/dịch vụ

        [Display(Name = "Đã hủy")]
        DaHuy = 4            // Bệnh nhân không khám hoặc có sự cố
    }

    // 2. Lớp chính Hồ sơ bệnh án
    public class HoSoBenhAn
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Ngày khám")]
        public DateTime NgayKham { get; set; } = DateTime.Now;

        [Display(Name = "Ngày tái khám")]
        public DateTime? NgayTaiKham { get; set; }

        [Display(Name = "Trạng thái")]
        public TrangThaiHoSo TrangThai { get; set; } = TrangThaiHoSo.ChoKham;

        // --- LIÊN KẾT HỆ THỐNG ---
        [Required(ErrorMessage = "Vui lòng chọn bệnh nhân")]
        [Display(Name = "Bệnh nhân")]
        public int BenhNhanId { get; set; }
        [ForeignKey("BenhNhanId")]
        public virtual BenhNhan? BenhNhan { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn bác sĩ")]
        [Display(Name = "Bác sĩ đảm nhận")]
        public int BacSiId { get; set; }
        [ForeignKey("BacSiId")]
        public virtual BacSi? BacSi { get; set; }

        // --- THÔNG TIN CHUYÊN MÔN DA LIỄU ---
        [Display(Name = "Tình trạng da")]
        public string? TinhTrangDa { get; set; } // Ví dụ: Da dầu, da khô, da hỗn hợp...

        [Display(Name = "Vị trí tổn thương")]
        public string? ViTriTonThuong { get; set; } // Ví dụ: Hai bên má, vùng lưng...

        [Display(Name = "Mức độ")]
        public string? MucDo { get; set; } // Ví dụ: Nhẹ, Trung bình, Nặng

        [Display(Name = "Triệu chứng lâm sàng")]
        public string? TrieuChung { get; set; }

        [Display(Name = "Chẩn đoán")]

        public string? ChanDoan { get; set; }

        [Display(Name = "Lời dặn của bác sĩ")]
        public string? LoiDan { get; set; }

        // --- CÁC DANH SÁCH LIÊN KẾT CHI TIẾT ---
        // Thêm vào trong class HoSoBenhAn
        [Display(Name = "Mã lịch hẹn")]
        public int? LichHenId { get; set; } // Nullable vì khách vãng lai không có lịch hẹn
        [ForeignKey("LichHenId")]
        public virtual LichHen? LichHen { get; set; }

        [Display(Name = "Ca làm việc")]
        public int? LichLamViecId { get; set; } // Để biết hồ sơ này thuộc ca trực nào
        [ForeignKey("LichLamViecId")]
        public virtual LichLamViec? LichLamViec { get; set; }

        // Liên kết 1-N tới Hình ảnh (Ảnh chính để so sánh & các ảnh phụ chi tiết)
        [Display(Name = "Hình ảnh bệnh án")]
        public virtual ICollection<HinhAnhBenhAn>? HinhAnhBenhAns { get; set; }
        // Trong file Models/HoSoBenhAn.cs

        [Display(Name = "Khung giờ bắt đầu")]
        public TimeSpan? KhungGioBatDau { get; set; } // Thêm dòng này

        // Liên kết 1-N tới bảng Chi tiết dịch vụ (Peel da, Laser, Lấy nhân mụn...)
        [Display(Name = "Dịch vụ thực hiện")]
        public virtual ICollection<ChiTietDichVu>? ChiTietDichVus { get; set; }

        // Liên kết 1-N tới bảng Chi tiết đơn thuốc
        [Display(Name = "Đơn thuốc")]
        public virtual ICollection<ChiTietDonThuoc>? ChiTietDonThuocs { get; set; }
    }
}