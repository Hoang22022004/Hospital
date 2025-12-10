using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hospital.Migrations
{
    /// <inheritdoc />
    public partial class FinalizeLichHenConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LichHen_BacSi_BacSiId",
                table: "LichHen");

            migrationBuilder.DropForeignKey(
                name: "FK_LichHen_DichVu_DichVuId",
                table: "LichHen");

            migrationBuilder.DropColumn(
                name: "ThoiGianMoiLanKhamPhut",
                table: "LichLamViec");

            migrationBuilder.RenameColumn(
                name: "IsAvailable",
                table: "LichLamViec",
                newName: "IsActive");

            migrationBuilder.AlterColumn<DateTime>(
                name: "GioKetThuc",
                table: "LichLamViec",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "time");

            migrationBuilder.AlterColumn<DateTime>(
                name: "GioBatDau",
                table: "LichLamViec",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "time");

            migrationBuilder.AlterColumn<string>(
                name: "TrieuChung",
                table: "LichHen",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "LichHen",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "HinhAnhUrl",
                table: "BacSi",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddForeignKey(
                name: "FK_LichHen_BacSi_BacSiId",
                table: "LichHen",
                column: "BacSiId",
                principalTable: "BacSi",
                principalColumn: "BacSiId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LichHen_DichVu_DichVuId",
                table: "LichHen",
                column: "DichVuId",
                principalTable: "DichVu",
                principalColumn: "DichVuId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LichHen_BacSi_BacSiId",
                table: "LichHen");

            migrationBuilder.DropForeignKey(
                name: "FK_LichHen_DichVu_DichVuId",
                table: "LichHen");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "LichLamViec",
                newName: "IsAvailable");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "GioKetThuc",
                table: "LichLamViec",
                type: "time",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "GioBatDau",
                table: "LichLamViec",
                type: "time",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<int>(
                name: "ThoiGianMoiLanKhamPhut",
                table: "LichLamViec",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "TrieuChung",
                table: "LichHen",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "LichHen",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "HinhAnhUrl",
                table: "BacSi",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LichHen_BacSi_BacSiId",
                table: "LichHen",
                column: "BacSiId",
                principalTable: "BacSi",
                principalColumn: "BacSiId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LichHen_DichVu_DichVuId",
                table: "LichHen",
                column: "DichVuId",
                principalTable: "DichVu",
                principalColumn: "DichVuId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
