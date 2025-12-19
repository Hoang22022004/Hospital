using System.ComponentModel.DataAnnotations;

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
        public string SoDienThoai { get; set; } // Dùng để tìm kiếm khách cũ

        // Mình giữ lại Ngày sinh (thay vì Năm sinh) để nhìn chuyên nghiệp hơn
        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime? NgaySinh { get; set; }

        [Display(Name = "Giới tính")]
        public string? GioiTinh { get; set; } // Nam/Nữ

        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string? Email { get; set; } // Có thể để trống nếu khách không có

        // --- QUAN TRỌNG NHẤT VỚI DA LIỄU ---
        [Display(Name = "Tiền sử bệnh / Dị ứng")]
        public string? TienSuBenh { get; set; } // VD: Dị ứng BHA, da mỏng yếu...

        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;
    }
}