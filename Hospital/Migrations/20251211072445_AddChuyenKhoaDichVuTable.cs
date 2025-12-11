using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hospital.Migrations
{
    /// <inheritdoc />
    public partial class AddChuyenKhoaDichVuTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateIndex(
                name: "IX_ChuyenKhoaDichVus_DichVuId",
                table: "ChuyenKhoaDichVus",
                column: "DichVuId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChuyenKhoaDichVus");
        }
    }
}
