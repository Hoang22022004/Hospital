using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hospital.Migrations
{
    /// <inheritdoc />
    public partial class LinkBenhLyToHoSo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ChanDoan",
                table: "HoSoBenhAn",
                type: "nvarchar(10)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HoSoBenhAn_ChanDoan",
                table: "HoSoBenhAn",
                column: "ChanDoan");

            migrationBuilder.AddForeignKey(
                name: "FK_HoSoBenhAn_BenhLy_ChanDoan",
                table: "HoSoBenhAn",
                column: "ChanDoan",
                principalTable: "BenhLy",
                principalColumn: "BenhLyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HoSoBenhAn_BenhLy_ChanDoan",
                table: "HoSoBenhAn");

            migrationBuilder.DropIndex(
                name: "IX_HoSoBenhAn_ChanDoan",
                table: "HoSoBenhAn");

            migrationBuilder.AlterColumn<string>(
                name: "ChanDoan",
                table: "HoSoBenhAn",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldNullable: true);
        }
    }
}
