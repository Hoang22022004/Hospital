using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Hospital.Models;

namespace Hospital.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Khai báo DbSet cho các Models cốt lõi của đề tài
        public DbSet<DichVu> DichVu { get; set; }
        public DbSet<ApplicationUser> ApplicationUser { get; set; }
        public DbSet<BacSi> BacSi { get; set; }
        public DbSet<BenhLy> BenhLy { get; set; }
        public DbSet<PhacDoDieuTri> PhacDoDieuTri { get; set; }
        public DbSet<LichLamViec> LichLamViec { get; set; }
        public DbSet<LichHen> LichHen { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Cấu hình Fluent API cho mối quan hệ BacSi và ApplicationUser (1-0...1)
            builder.Entity<BacSi>()
                .HasOne(b => b.User)
                .WithOne(u => u.BacSiProfile)
                .HasForeignKey<BacSi>(b => b.IdentityUserId);

            // ***************************************************************
            // CẤU HÌNH KHẮC PHỤC LỖI MULTIPLE CASCADE PATHS (Tối ưu)
            // ***************************************************************

            // Vô hiệu hóa CASCADE trên mối quan hệ LichLamViec <-> LichHen.
            // Khi xóa LichLamViec, KHÔNG tự động xóa LichHen liên quan.
            builder.Entity<LichHen>()
                .HasOne(lh => lh.LichLamViec)
                .WithMany(llv => llv.LichHens)
                .HasForeignKey(lh => lh.LichLamViecId)
                .OnDelete(DeleteBehavior.Restrict);

            // Vô hiệu hóa CASCADE trên mối quan hệ BacSi <-> LichHen.
            // Khi xóa Bác sĩ, KHÔNG tự động xóa Lịch hẹn, mà cần xóa thủ công (hoặc hiển thị lỗi).
            builder.Entity<LichHen>()
                .HasOne(lh => lh.BacSi)
                .WithMany() // Nếu không định nghĩa Navigation Property ngược lại trong BacSi
                .HasForeignKey(lh => lh.BacSiId)
                .OnDelete(DeleteBehavior.Restrict);

            // Vô hiệu hóa CASCADE trên mối quan hệ DichVu <-> LichHen.
            // Khi xóa Dịch vụ, KHÔNG tự động xóa Lịch hẹn liên quan.
            builder.Entity<LichHen>()
               .HasOne(lh => lh.DichVu)
               .WithMany() // Nếu không định nghĩa Navigation Property ngược lại trong DichVu
               .HasForeignKey(lh => lh.DichVuId)
               .OnDelete(DeleteBehavior.Restrict);

            // ***************************************************************
        }
    }
}