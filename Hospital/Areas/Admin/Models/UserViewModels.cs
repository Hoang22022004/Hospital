using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Hospital.Areas.Admin.Models
{
    // 1. Dùng để hiển thị danh sách (Index)
    public class UserViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public bool IsLocked { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
    }

    // 2. Dùng để tạo mới (Create)
    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Họ tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Mật khẩu")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Vai trò")]
        [Display(Name = "Vai trò")]
        public string Role { get; set; }

        // Danh sách Role để chọn
        public IEnumerable<SelectListItem>? RoleList { get; set; }
    } // <--- ĐÓNG NGOẶC CỦA CreateUserViewModel TẠI ĐÂY

    // 3. Dùng để chỉnh sửa (Edit) - NẰM NGOÀI, NGANG HÀNG
    public class EditUserViewModel
    {
        public string Id { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Họ tên")]
        [Display(Name = "Họ và Tên")]
        public string FullName { get; set; }

        [Display(Name = "Vai trò")]
        public string Role { get; set; }

        [Display(Name = "Mật khẩu mới")]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        public IEnumerable<SelectListItem>? RoleList { get; set; }
    }
}