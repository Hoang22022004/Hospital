using System.ComponentModel.DataAnnotations;

namespace Hospital.Models
{
    public class TinTuc
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề bài viết")]
        [Display(Name = "Tiêu đề")]
        public string TieuDe { get; set; }

        [Required(ErrorMessage = "Nội dung không được để trống")]
        [Display(Name = "Nội dung bài viết")]
        public string NoiDung { get; set; }

        [Display(Name = "Hình ảnh minh họa")]
        public string? HinhAnhUrl { get; set; }

        [Display(Name = "Ngày đăng")]
        public DateTime NgayDang { get; set; } = DateTime.Now;

        [Display(Name = "Tác giả")]
        public string? TacGia { get; set; }

        [Display(Name = "Danh mục")]
        public string? DanhMuc { get; set; } // Ví dụ: Kiến thức da liễu, Tin tức phòng khám

        [Display(Name = "Hiển thị")]
        public bool IsPublished { get; set; } = true;
    }
}