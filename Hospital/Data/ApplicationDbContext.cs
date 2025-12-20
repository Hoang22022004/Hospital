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

        // --- MODELS CỐT LÕI CỦA BẠN ---
        public DbSet<DichVu> DichVu { get; set; }
        public DbSet<ApplicationUser> ApplicationUser { get; set; }
        public DbSet<BacSi> BacSi { get; set; }
        public DbSet<BenhLy> BenhLy { get; set; }
        public DbSet<PhacDoDieuTri> PhacDoDieuTri { get; set; }
        public DbSet<LichLamViec> LichLamViec { get; set; }
        public DbSet<LichHen> LichHen { get; set; }
        public DbSet<ChuyenKhoa> ChuyenKhoa { get; set; }
        public DbSet<ChuyenKhoaDichVu> ChuyenKhoaDichVus { get; set; }
        public DbSet<Thuoc> Thuoc { get; set; }
        public DbSet<BenhNhan> BenhNhan { get; set; }

        // --- MỚI THÊM: QUẢN LÝ HỒ SƠ BỆNH ÁN CHI TIẾT ---
        public DbSet<HoSoBenhAn> HoSoBenhAn { get; set; }
        public DbSet<HinhAnhBenhAn> HinhAnhBenhAn { get; set; }
        public DbSet<ChiTietDichVu> ChiTietDichVu { get; set; }
        public DbSet<ChiTietDonThuoc> ChiTietDonThuoc { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 1. Cấu hình cũ: BacSi và ApplicationUser
            builder.Entity<BacSi>()
                .HasOne(b => b.User)
                .WithOne(u => u.BacSiProfile)
                .HasForeignKey<BacSi>(b => b.IdentityUserId);

            // 2. Cấu hình cũ: ChuyenKhoa <-> Bác sĩ
            builder.Entity<BacSi>()
                .HasOne(b => b.ChuyenKhoa)
                .WithMany(c => c.BacSis)
                .HasForeignKey(b => b.ChuyenKhoaId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3. Cấu hình cũ: ChuyenKhoa <-> Dịch vụ (N-N)
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

            // 4. Cấu hình cũ: Lịch hẹn
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

            // 5. Cấu hình cũ: Bệnh nhân (Số điện thoại Unique)
            builder.Entity<BenhNhan>()
                .HasIndex(b => b.SoDienThoai)
                .IsUnique();

            // ************************************************************
            // ********** CẤU HÌNH MỚI: HỆ THỐNG HỒ SƠ BỆNH ÁN ************
            // ************************************************************

            // A. Hình ảnh <-> Hồ sơ bệnh án
            builder.Entity<HinhAnhBenhAn>()
                .HasOne(hi => hi.HoSoBenhAn)
                .WithMany(h => h.HinhAnhBenhAns)
                .HasForeignKey(hi => hi.HoSoBenhAnId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa hồ sơ thì xóa hết ảnh liên quan

            // B. Chi tiết dịch vụ (N-N giữa Hồ sơ và Dịch vụ)
            builder.Entity<ChiTietDichVu>()
                .HasOne(ct => ct.HoSoBenhAn)
                .WithMany(h => h.ChiTietDichVus)
                .HasForeignKey(ct => ct.HoSoBenhAnId);

            builder.Entity<ChiTietDichVu>()
                .HasOne(ct => ct.DichVu)
                .WithMany()
                .HasForeignKey(ct => ct.DichVuId)
                .OnDelete(DeleteBehavior.Restrict); // Không cho xóa dịch vụ nếu đã có trong bệnh án

            // C. Chi tiết đơn thuốc (N-N giữa Hồ sơ và Thuốc)
            builder.Entity<ChiTietDonThuoc>()
                .HasOne(ct => ct.HoSoBenhAn)
                .WithMany(h => h.ChiTietDonThuocs)
                .HasForeignKey(ct => ct.HoSoBenhAnId);

            builder.Entity<ChiTietDonThuoc>()
                .HasOne(ct => ct.Thuoc)
                .WithMany()
                .HasForeignKey(ct => ct.ThuocId)
                .OnDelete(DeleteBehavior.Restrict); // Không cho xóa thuốc nếu đã kê trong bệnh án

            // D. Hồ sơ bệnh án <-> Bệnh nhân (Gắn kết lịch sử)
            builder.Entity<HoSoBenhAn>()
                .HasOne(h => h.BenhNhan)
                .WithMany(b => b.HoSoBenhAns)
                .HasForeignKey(h => h.BenhNhanId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}