using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Hospital.Models
{
    // Kế thừa từ IdentityUser để có sẵn các thuộc tính:
    // Id, UserName, NormalizedUserName, Email, PhoneNumber, PasswordHash, v.v.
    public class ApplicationUser : IdentityUser
    {
        // Thêm các thuộc tính tùy chỉnh cho người dùng của bệnh viện

        // Ví dụ: Tên đầy đủ
        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        // Ví dụ: Địa chỉ (Nếu cần)
        [StringLength(255)]
        public string Address { get; set; }

        // Ví dụ: Ngày sinh (Nếu cần cho hồ sơ bệnh nhân)
        public DateTime? DateOfBirth { get; set; }

        // *************************************************************
        // Mối quan hệ với bảng BacSi (Quan trọng cho tài khoản Bác sĩ)
        // Đây là Navigation Property để liên kết Tài khoản với Hồ sơ Bác sĩ
        // Nếu một tài khoản có Role là Doctor, nó sẽ liên kết với một hồ sơ BacSi
        public BacSi? BacSiProfile { get; set; }
        // *************************************************************
    }
}