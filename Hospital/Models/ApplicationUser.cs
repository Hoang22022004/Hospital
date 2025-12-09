using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Hospital.Models
{
    // Kế thừa từ IdentityUser
    public class ApplicationUser : IdentityUser
    {
        // TRƯỚC: [Required] public string FullName { get; set; }
        // SAU: BỎ [Required] và thêm dấu ? để cho phép NULL
        [StringLength(100)]
        public string? FullName { get; set; } // ⬅️ Sửa thành public string?

        // Đã sửa ở lần trước, giữ nguyên để cho phép NULL
        [StringLength(255)]
        public string? Address { get; set; } // ⬅️ Giữ nguyên public string?

        // Ví dụ: Ngày sinh
        public DateTime? DateOfBirth { get; set; }

        // Mối quan hệ với bảng BacSi
        public BacSi? BacSiProfile { get; set; }
    }
}