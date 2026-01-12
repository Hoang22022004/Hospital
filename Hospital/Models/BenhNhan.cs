using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Thêm thư viện này

namespace Hospital.Models
{
    public class BenhNhan
    {
        [Key]
        public int BenhNhanId { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [Display(Name = "Họ và Tên")]
        [StringLength(100)]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [StringLength(15, ErrorMessage = "SĐT không hợp lệ")]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; }

        // --- TRƯỜNG CẦN BỔ SUNG ĐỂ SỬA LỖI ---
        // Lưu ID từ bảng Identity User để biết khách này đã có tài khoản hay chưa
        [Display(Name = "Mã tài khoản")]
        public string? IdentityUserId { get; set; }

        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime? NgaySinh { get; set; }

        [Display(Name = "Giới tính")]
        public string? GioiTinh { get; set; }

        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string? Email { get; set; }

        [Display(Name = "Tiền sử bệnh / Dị ứng")]
        public string? TienSuBenh { get; set; }

        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        [Display(Name = "Lịch sử khám bệnh")]
        public virtual ICollection<HoSoBenhAn>? HoSoBenhAns { get; set; }

        public BenhNhan()
        {
            HoSoBenhAns = new HashSet<HoSoBenhAn>();
        }
    }
}