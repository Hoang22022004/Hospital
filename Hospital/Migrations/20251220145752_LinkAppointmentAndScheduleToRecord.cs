using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hospital.Migrations
{
    /// <inheritdoc />
    public partial class LinkAppointmentAndScheduleToRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LichHenId",
                table: "HoSoBenhAn",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LichLamViecId",
                table: "HoSoBenhAn",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HoSoBenhAn_LichHenId",
                table: "HoSoBenhAn",
                column: "LichHenId");

            migrationBuilder.CreateIndex(
                name: "IX_HoSoBenhAn_LichLamViecId",
                table: "HoSoBenhAn",
                column: "LichLamViecId");

            migrationBuilder.AddForeignKey(
                name: "FK_HoSoBenhAn_LichHen_LichHenId",
                table: "HoSoBenhAn",
                column: "LichHenId",
                principalTable: "LichHen",
                principalColumn: "LichHenId");

            migrationBuilder.AddForeignKey(
                name: "FK_HoSoBenhAn_LichLamViec_LichLamViecId",
                table: "HoSoBenhAn",
                column: "LichLamViecId",
                principalTable: "LichLamViec",
                principalColumn: "LichLamViecId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HoSoBenhAn_LichHen_LichHenId",
                table: "HoSoBenhAn");

            migrationBuilder.DropForeignKey(
                name: "FK_HoSoBenhAn_LichLamViec_LichLamViecId",
                table: "HoSoBenhAn");

            migrationBuilder.DropIndex(
                name: "IX_HoSoBenhAn_LichHenId",
                table: "HoSoBenhAn");

            migrationBuilder.DropIndex(
                name: "IX_HoSoBenhAn_LichLamViecId",
                table: "HoSoBenhAn");

            migrationBuilder.DropColumn(
                name: "LichHenId",
                table: "HoSoBenhAn");

            migrationBuilder.DropColumn(
                name: "LichLamViecId",
                table: "HoSoBenhAn");
        }
    }
}
