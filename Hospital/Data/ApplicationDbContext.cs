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
        public DbSet<ChuyenKhoa> ChuyenKhoa { get; set; }
        public DbSet<ChuyenKhoaDichVu> ChuyenKhoaDichVus { get; set; }

        // ********** QUAN TRỌNG: THÊM DÒNG NÀY ĐỂ TẠO BẢNG THUỐC **********
        public DbSet<Thuoc> Thuoc { get; set; }
        // ***************************************************************

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Cấu hình Fluent API cho mối quan hệ BacSi và ApplicationUser
            builder.Entity<BacSi>()
                .HasOne(b => b.User)
                .WithOne(u => u.BacSiProfile)
                .HasForeignKey<BacSi>(b => b.IdentityUserId);

            // Cấu hình Chuyên khoa <-> Bác sĩ
            builder.Entity<BacSi>()
                .HasOne(b => b.ChuyenKhoa)
                .WithMany(c => c.BacSis)
                .HasForeignKey(b => b.ChuyenKhoaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cấu hình Chuyên khoa <-> Dịch vụ (N-N)
            builder.Entity<ChuyenKhoaDichVu>()
                .HasKey(cd => new { cd.ChuyenKhoaId, cd.DichVuId });

            builder.Entity<ChuyenKhoaDichVu>()
                .HasOne(cd => cd.ChuyenKhoa)
                .WithMany(c => c.ChuyenKhoaDichVus)
                .HasForeignKey(cd => cd.ChuyenKhoaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ChuyenKhoaDichVu>()
                .HasOne(cd => cd.DichVu)
                .WithMany(d => d.ChuyenKhoaDichVus)
                .HasForeignKey(cd => cd.DichVuId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cấu hình Lịch hẹn
            builder.Entity<LichHen>()
                .HasOne(lh => lh.LichLamViec)
                .WithMany(llv => llv.LichHens)
                .HasForeignKey(lh => lh.LichLamViecId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<LichHen>()
                .HasOne(lh => lh.BacSi)
                .WithMany()
                .HasForeignKey(lh => lh.BacSiId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<LichHen>()
                .HasOne(lh => lh.DichVu)
                .WithMany()
                .HasForeignKey(lh => lh.DichVuId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}