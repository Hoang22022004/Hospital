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

        // ********** ĐÃ THÊM: DbSet cho Chuyên khoa **********
        public DbSet<ChuyenKhoa> ChuyenKhoa { get; set; }

        // ********** ĐÃ THÊM MỚI: DbSet cho bảng trung gian N-N **********
        public DbSet<ChuyenKhoaDichVu> ChuyenKhoaDichVus { get; set; }
        // ***************************************************************

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Cấu hình Fluent API cho mối quan hệ BacSi và ApplicationUser (1-0...1)
            builder.Entity<BacSi>()
                .HasOne(b => b.User)
                .WithOne(u => u.BacSiProfile)
                .HasForeignKey<BacSi>(b => b.IdentityUserId);

            // ***************************************************************
            // CẤU HÌNH LIÊN KẾT: CHUYÊN KHOA <-> BÁC SĨ (1-N)
            // ***************************************************************
            builder.Entity<BacSi>()
                .HasOne(b => b.ChuyenKhoa)
                .WithMany(c => c.BacSis)
                .HasForeignKey(b => b.ChuyenKhoaId)
                .OnDelete(DeleteBehavior.Restrict);


            // ***************************************************************
            // CẤU HÌNH MỚI: CHUYÊN KHOA <-> DỊCH VỤ (N-N)
            // ***************************************************************
            builder.Entity<ChuyenKhoaDichVu>()
                .HasKey(cd => new { cd.ChuyenKhoaId, cd.DichVuId }); // Thiết lập khóa chính kép

            builder.Entity<ChuyenKhoaDichVu>()
                .HasOne(cd => cd.ChuyenKhoa)
                .WithMany(c => c.ChuyenKhoaDichVus)
                .HasForeignKey(cd => cd.ChuyenKhoaId)
                .OnDelete(DeleteBehavior.Restrict); // Vô hiệu hóa Cascade Delete

            builder.Entity<ChuyenKhoaDichVu>()
                .HasOne(cd => cd.DichVu)
                .WithMany(d => d.ChuyenKhoaDichVus)
                .HasForeignKey(cd => cd.DichVuId)
                .OnDelete(DeleteBehavior.Restrict); // Vô hiệu hóa Cascade Delete

            // ***************************************************************
            // CẤU HÌNH LIÊN KẾT: LỊCH HẸN (Vô hiệu hóa CASCADE PATHS)
            // ***************************************************************

            // LichLamViec <-> LichHen.
            builder.Entity<LichHen>()
                .HasOne(lh => lh.LichLamViec)
                .WithMany(llv => llv.LichHens)
                .HasForeignKey(lh => lh.LichLamViecId)
                .OnDelete(DeleteBehavior.Restrict);

            // BacSi <-> LichHen.
            builder.Entity<LichHen>()
                .HasOne(lh => lh.BacSi)
                .WithMany()
                .HasForeignKey(lh => lh.BacSiId)
                .OnDelete(DeleteBehavior.Restrict);

            // DichVu <-> LichHen.
            builder.Entity<LichHen>()
               .HasOne(lh => lh.DichVu)
               .WithMany()
               .HasForeignKey(lh => lh.DichVuId)
               .OnDelete(DeleteBehavior.Restrict);

            // ***************************************************************
        }
    }
}