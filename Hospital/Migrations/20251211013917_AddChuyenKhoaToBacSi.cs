using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hospital.Migrations
{
    /// <inheritdoc />
    public partial class AddChuyenKhoaToBacSi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. THÊM CỘT ChuyenKhoaId VÀO BẢNG BacSi
            migrationBuilder.AddColumn<int>(
                name: "ChuyenKhoaId",
                table: "BacSi",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // 2. TẠO BẢNG ChuyenKhoa
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

            // ***************************************************************
            // ********** BƯỚC KHẮC PHỤC LỖI TÍNH TOÀN VẸN DỮ LIỆU **********
            // ***************************************************************

            // 3a. CHÈN BẢN GHI CHUYÊN KHOA MẪU ĐẦU TIÊN (ID = 1)
            // Lệnh này đảm bảo bản ghi có ID=1 tồn tại, dùng cho bước cập nhật BacSi.
            migrationBuilder.InsertData(
                table: "ChuyenKhoa",
                columns: new[] { "ChuyenKhoaId", "TenChuyenKhoa", "MoTa" },
                values: new object[] { 1, "Da liễu tổng quát (Mặc định)", "Chuyên khoa được gán mặc định cho các Bác sĩ đã có sẵn." }
            );

            // 3b. CẬP NHẬT DỮ LIỆU CŨ: Gán ID Chuyên khoa MỚI (ID = 1) cho tất cả các bản ghi BacSi đã tồn tại
            // Lệnh này giải quyết xung đột FOREIGN KEY.
            migrationBuilder.Sql("UPDATE BacSi SET ChuyenKhoaId = 1 WHERE ChuyenKhoaId = 0 OR ChuyenKhoaId IS NULL");

            // ***************************************************************
            // ***************************************************************


            // 4. THÊM INDEX
            migrationBuilder.CreateIndex(
                name: "IX_BacSi_ChuyenKhoaId",
                table: "BacSi",
                column: "ChuyenKhoaId");

            // 5. THÊM FOREIGN KEY (Bây giờ sẽ thành công vì tất cả các giá trị đều là 1)
            migrationBuilder.AddForeignKey(
                name: "FK_BacSi_ChuyenKhoa_ChuyenKhoaId",
                table: "BacSi",
                column: "ChuyenKhoaId",
                principalTable: "ChuyenKhoa",
                principalColumn: "ChuyenKhoaId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Đảm bảo có thể rollback (xóa các bước bổ sung trong Up)
            migrationBuilder.DropForeignKey(
                name: "FK_BacSi_ChuyenKhoa_ChuyenKhoaId",
                table: "BacSi");

            migrationBuilder.DropTable(
                name: "ChuyenKhoa");

            migrationBuilder.DropIndex(
                name: "IX_BacSi_ChuyenKhoaId",
                table: "BacSi");

            migrationBuilder.DropColumn(
                name: "ChuyenKhoaId",
                table: "BacSi");
        }
    }
}