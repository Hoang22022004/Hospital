using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hospital.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePrescriptionDetailsV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LieuDung",
                table: "ChiTietDonThuoc",
                newName: "LieuTrua");

            migrationBuilder.AlterColumn<double>(
                name: "SoLuong",
                table: "ChiTietDonThuoc",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "CachDungTongHop",
                table: "ChiTietDonThuoc",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GhiChu",
                table: "ChiTietDonThuoc",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LieuChieu",
                table: "ChiTietDonThuoc",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LieuSang",
                table: "ChiTietDonThuoc",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LieuToi",
                table: "ChiTietDonThuoc",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SoNgayDung",
                table: "ChiTietDonThuoc",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CachDungTongHop",
                table: "ChiTietDonThuoc");

            migrationBuilder.DropColumn(
                name: "GhiChu",
                table: "ChiTietDonThuoc");

            migrationBuilder.DropColumn(
                name: "LieuChieu",
                table: "ChiTietDonThuoc");

            migrationBuilder.DropColumn(
                name: "LieuSang",
                table: "ChiTietDonThuoc");

            migrationBuilder.DropColumn(
                name: "LieuToi",
                table: "ChiTietDonThuoc");

            migrationBuilder.DropColumn(
                name: "SoNgayDung",
                table: "ChiTietDonThuoc");

            migrationBuilder.RenameColumn(
                name: "LieuTrua",
                table: "ChiTietDonThuoc",
                newName: "LieuDung");

            migrationBuilder.AlterColumn<int>(
                name: "SoLuong",
                table: "ChiTietDonThuoc",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");
        }
    }
}
