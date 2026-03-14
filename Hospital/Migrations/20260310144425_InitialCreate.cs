using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hospital.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BenhLy",
                columns: table => new
                {
                    BenhLyId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TenBenhLy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MoTaTongQuan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrieuChung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhuongPhapDieuTri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HinhAnhUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BenhLy", x => x.BenhLyId);
                });

            migrationBuilder.CreateTable(
                name: "BenhNhan",
                columns: table => new
                {
                    BenhNhanId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    IdentityUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgaySinh = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GioiTinh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TienSuBenh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BenhNhan", x => x.BenhNhanId);
                });

            migrationBuilder.CreateTable(
                name: "ChuyenKhoa",
                columns: table => new
                {
                    ChuyenKhoaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenChuyenKhoa = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChuyenKhoa", x => x.ChuyenKhoaId);
                });

            migrationBuilder.CreateTable(
                name: "DichVu",
                columns: table => new
                {
                    DichVuId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDichVu = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MoTaNgan = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MoTaChiTiet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Gia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AnhDichVuUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsHot = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DichVu", x => x.DichVuId);
                });

            migrationBuilder.CreateTable(
                name: "Thuoc",
                columns: table => new
                {
                    ThuocId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenThuoc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThuongHieu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HoatChat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CongDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DonViTinh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GiaNhap = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GiaBan = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SoLuongTon = table.Column<int>(type: "int", nullable: false),
                    HanSuDung = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HinhAnhUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CachDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    PhanLoai = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Thuoc", x => x.ThuocId);
                });

            migrationBuilder.CreateTable(
                name: "TinTuc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TieuDe = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HinhAnhUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayDang = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TacGia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DanhMuc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TinTuc", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PhacDoDieuTri",
                columns: table => new
                {
                    PhacDoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BenhLyId = table.Column<string>(type: "nvarchar(10)", nullable: false),
                    TenThuocPhuongPhap = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LieuLuong = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TacDungPhu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhacDoDieuTri", x => x.PhacDoId);
                    table.ForeignKey(
                        name: "FK_PhacDoDieuTri_BenhLy_BenhLyId",
                        column: x => x.BenhLyId,
                        principalTable: "BenhLy",
                        principalColumn: "BenhLyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BacSi",
                columns: table => new
                {
                    BacSiId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoTen = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ChuyenMon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BangCap = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MoTaChiTiet = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HinhAnhUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ChuyenKhoaId = table.Column<int>(type: "int", nullable: false),
                    IdentityUserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BacSi", x => x.BacSiId);
                    table.ForeignKey(
                        name: "FK_BacSi_AspNetUsers_IdentityUserId",
                        column: x => x.IdentityUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BacSi_ChuyenKhoa_ChuyenKhoaId",
                        column: x => x.ChuyenKhoaId,
                        principalTable: "ChuyenKhoa",
                        principalColumn: "ChuyenKhoaId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChuyenKhoaDichVus",
                columns: table => new
                {
                    ChuyenKhoaId = table.Column<int>(type: "int", nullable: false),
                    DichVuId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChuyenKhoaDichVus", x => new { x.ChuyenKhoaId, x.DichVuId });
                    table.ForeignKey(
                        name: "FK_ChuyenKhoaDichVus_ChuyenKhoa_ChuyenKhoaId",
                        column: x => x.ChuyenKhoaId,
                        principalTable: "ChuyenKhoa",
                        principalColumn: "ChuyenKhoaId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChuyenKhoaDichVus_DichVu_DichVuId",
                        column: x => x.DichVuId,
                        principalTable: "DichVu",
                        principalColumn: "DichVuId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LichLamViec",
                columns: table => new
                {
                    LichLamViecId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NgayLamViec = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GioBatDau = table.Column<TimeSpan>(type: "time", nullable: false),
                    GioKetThuc = table.Column<TimeSpan>(type: "time", nullable: false),
                    ThoiLuongKhungGioPhut = table.Column<int>(type: "int", nullable: false),
                    BacSiId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichLamViec", x => x.LichLamViecId);
                    table.ForeignKey(
                        name: "FK_LichLamViec_BacSi_BacSiId",
                        column: x => x.BacSiId,
                        principalTable: "BacSi",
                        principalColumn: "BacSiId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LichHen",
                columns: table => new
                {
                    LichHenId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenKhachHang = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KhungGioBatDau = table.Column<TimeSpan>(type: "time", nullable: false),
                    LichLamViecId = table.Column<int>(type: "int", nullable: false),
                    BacSiId = table.Column<int>(type: "int", nullable: false),
                    DichVuId = table.Column<int>(type: "int", nullable: false),
                    TrieuChung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThoiGianDat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichHen", x => x.LichHenId);
                    table.ForeignKey(
                        name: "FK_LichHen_BacSi_BacSiId",
                        column: x => x.BacSiId,
                        principalTable: "BacSi",
                        principalColumn: "BacSiId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LichHen_DichVu_DichVuId",
                        column: x => x.DichVuId,
                        principalTable: "DichVu",
                        principalColumn: "DichVuId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LichHen_LichLamViec_LichLamViecId",
                        column: x => x.LichLamViecId,
                        principalTable: "LichLamViec",
                        principalColumn: "LichLamViecId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HoSoBenhAn",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NgayKham = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayTaiKham = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    BenhNhanId = table.Column<int>(type: "int", nullable: false),
                    BacSiId = table.Column<int>(type: "int", nullable: false),
                    TinhTrangDa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ViTriTonThuong = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MucDo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrieuChung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChanDoan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LoiDan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LichHenId = table.Column<int>(type: "int", nullable: true),
                    LichLamViecId = table.Column<int>(type: "int", nullable: true),
                    KhungGioBatDau = table.Column<TimeSpan>(type: "time", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoSoBenhAn", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoSoBenhAn_BacSi_BacSiId",
                        column: x => x.BacSiId,
                        principalTable: "BacSi",
                        principalColumn: "BacSiId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HoSoBenhAn_BenhNhan_BenhNhanId",
                        column: x => x.BenhNhanId,
                        principalTable: "BenhNhan",
                        principalColumn: "BenhNhanId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HoSoBenhAn_LichHen_LichHenId",
                        column: x => x.LichHenId,
                        principalTable: "LichHen",
                        principalColumn: "LichHenId");
                    table.ForeignKey(
                        name: "FK_HoSoBenhAn_LichLamViec_LichLamViecId",
                        column: x => x.LichLamViecId,
                        principalTable: "LichLamViec",
                        principalColumn: "LichLamViecId");
                });

            migrationBuilder.CreateTable(
                name: "ChiTietDichVu",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoSoBenhAnId = table.Column<int>(type: "int", nullable: false),
                    DichVuId = table.Column<int>(type: "int", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    DonGiaTaiThoiDiemKham = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietDichVu", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChiTietDichVu_DichVu_DichVuId",
                        column: x => x.DichVuId,
                        principalTable: "DichVu",
                        principalColumn: "DichVuId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChiTietDichVu_HoSoBenhAn_HoSoBenhAnId",
                        column: x => x.HoSoBenhAnId,
                        principalTable: "HoSoBenhAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietDonThuoc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoSoBenhAnId = table.Column<int>(type: "int", nullable: false),
                    ThuocId = table.Column<int>(type: "int", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    LieuDung = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietDonThuoc", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChiTietDonThuoc_HoSoBenhAn_HoSoBenhAnId",
                        column: x => x.HoSoBenhAnId,
                        principalTable: "HoSoBenhAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietDonThuoc_Thuoc_ThuocId",
                        column: x => x.ThuocId,
                        principalTable: "Thuoc",
                        principalColumn: "ThuocId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HinhAnhBenhAn",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DuongDan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LaAnhChinh = table.Column<bool>(type: "bit", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HoSoBenhAnId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HinhAnhBenhAn", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HinhAnhBenhAn_HoSoBenhAn_HoSoBenhAnId",
                        column: x => x.HoSoBenhAnId,
                        principalTable: "HoSoBenhAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BacSi_ChuyenKhoaId",
                table: "BacSi",
                column: "ChuyenKhoaId");

            migrationBuilder.CreateIndex(
                name: "IX_BacSi_IdentityUserId",
                table: "BacSi",
                column: "IdentityUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BenhNhan_SoDienThoai",
                table: "BenhNhan",
                column: "SoDienThoai",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDichVu_DichVuId",
                table: "ChiTietDichVu",
                column: "DichVuId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDichVu_HoSoBenhAnId",
                table: "ChiTietDichVu",
                column: "HoSoBenhAnId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonThuoc_HoSoBenhAnId",
                table: "ChiTietDonThuoc",
                column: "HoSoBenhAnId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonThuoc_ThuocId",
                table: "ChiTietDonThuoc",
                column: "ThuocId");

            migrationBuilder.CreateIndex(
                name: "IX_ChuyenKhoaDichVus_DichVuId",
                table: "ChuyenKhoaDichVus",
                column: "DichVuId");

            migrationBuilder.CreateIndex(
                name: "IX_HinhAnhBenhAn_HoSoBenhAnId",
                table: "HinhAnhBenhAn",
                column: "HoSoBenhAnId");

            migrationBuilder.CreateIndex(
                name: "IX_HoSoBenhAn_BacSiId",
                table: "HoSoBenhAn",
                column: "BacSiId");

            migrationBuilder.CreateIndex(
                name: "IX_HoSoBenhAn_BenhNhanId",
                table: "HoSoBenhAn",
                column: "BenhNhanId");

            migrationBuilder.CreateIndex(
                name: "IX_HoSoBenhAn_LichHenId",
                table: "HoSoBenhAn",
                column: "LichHenId");

            migrationBuilder.CreateIndex(
                name: "IX_HoSoBenhAn_LichLamViecId",
                table: "HoSoBenhAn",
                column: "LichLamViecId");

            migrationBuilder.CreateIndex(
                name: "IX_LichHen_BacSiId",
                table: "LichHen",
                column: "BacSiId");

            migrationBuilder.CreateIndex(
                name: "IX_LichHen_DichVuId",
                table: "LichHen",
                column: "DichVuId");

            migrationBuilder.CreateIndex(
                name: "IX_LichHen_LichLamViecId",
                table: "LichHen",
                column: "LichLamViecId");

            migrationBuilder.CreateIndex(
                name: "IX_LichLamViec_BacSiId",
                table: "LichLamViec",
                column: "BacSiId");

            migrationBuilder.CreateIndex(
                name: "IX_PhacDoDieuTri_BenhLyId",
                table: "PhacDoDieuTri",
                column: "BenhLyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "ChiTietDichVu");

            migrationBuilder.DropTable(
                name: "ChiTietDonThuoc");

            migrationBuilder.DropTable(
                name: "ChuyenKhoaDichVus");

            migrationBuilder.DropTable(
                name: "HinhAnhBenhAn");

            migrationBuilder.DropTable(
                name: "PhacDoDieuTri");

            migrationBuilder.DropTable(
                name: "TinTuc");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Thuoc");

            migrationBuilder.DropTable(
                name: "HoSoBenhAn");

            migrationBuilder.DropTable(
                name: "BenhLy");

            migrationBuilder.DropTable(
                name: "BenhNhan");

            migrationBuilder.DropTable(
                name: "LichHen");

            migrationBuilder.DropTable(
                name: "DichVu");

            migrationBuilder.DropTable(
                name: "LichLamViec");

            migrationBuilder.DropTable(
                name: "BacSi");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "ChuyenKhoa");
        }
    }
}
