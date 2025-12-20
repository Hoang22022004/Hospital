using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hospital.Migrations
{
    /// <inheritdoc />
    public partial class AddFullMedicalRecordSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    ChanDoan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoiDan = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietDichVu");

            migrationBuilder.DropTable(
                name: "ChiTietDonThuoc");

            migrationBuilder.DropTable(
                name: "HinhAnhBenhAn");

            migrationBuilder.DropTable(
                name: "HoSoBenhAn");
        }
    }
}
