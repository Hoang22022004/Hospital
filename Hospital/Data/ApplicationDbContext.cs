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
            // CẤU HÌNH KHẮC PHỤC LỖI MULTIPLE CASCADE PATHS (RẤT QUAN TRỌNG)
            // ***************************************************************

            // Khi xóa một LichLamViec, KHÔNG tự động xóa LichHen liên quan.
            // Điều này vô hiệu hóa CASCADE trên mối quan hệ LichLamViec <-> LichHen,
            // giúp giải quyết xung đột với các mối quan hệ CASCADE khác (như BacSi <-> LichHen).
            builder.Entity<LichHen>()
                .HasOne(lh => lh.LichLamViec)
                .WithMany(llv => llv.LichHens)
                .HasForeignKey(lh => lh.LichLamViecId)
                .OnDelete(DeleteBehavior.Restrict); // Dùng Restrict (hoặc NoAction)
        }
    }
}