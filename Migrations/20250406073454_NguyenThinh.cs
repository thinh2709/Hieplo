using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyBenhVienNoiTru.Migrations
{
    /// <inheritdoc />
    public partial class NguyenThinh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TenKhoa",
                table: "Khoas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "GiuongMaGiuong",
                table: "BenhNhans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Giuongs",
                columns: table => new
                {
                    MaGiuong = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenGiuong = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GiaTheoNgay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaKhoa = table.Column<int>(type: "int", nullable: false),
                    MaBenhNhan = table.Column<int>(type: "int", nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    KhoaMaKhoa = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Giuongs", x => x.MaGiuong);
                    table.ForeignKey(
                        name: "FK_Giuongs_BenhNhans_MaBenhNhan",
                        column: x => x.MaBenhNhan,
                        principalTable: "BenhNhans",
                        principalColumn: "MaBenhNhan");
                    table.ForeignKey(
                        name: "FK_Giuongs_Khoas_KhoaMaKhoa",
                        column: x => x.KhoaMaKhoa,
                        principalTable: "Khoas",
                        principalColumn: "MaKhoa");
                    table.ForeignKey(
                        name: "FK_Giuongs_Khoas_MaKhoa",
                        column: x => x.MaKhoa,
                        principalTable: "Khoas",
                        principalColumn: "MaKhoa");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BenhNhans_GiuongMaGiuong",
                table: "BenhNhans",
                column: "GiuongMaGiuong");

            migrationBuilder.CreateIndex(
                name: "IX_Giuongs_KhoaMaKhoa",
                table: "Giuongs",
                column: "KhoaMaKhoa");

            migrationBuilder.CreateIndex(
                name: "IX_Giuongs_MaBenhNhan",
                table: "Giuongs",
                column: "MaBenhNhan",
                unique: true,
                filter: "[MaBenhNhan] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Giuongs_MaKhoa",
                table: "Giuongs",
                column: "MaKhoa");

            migrationBuilder.AddForeignKey(
                name: "FK_BenhNhans_Giuongs_GiuongMaGiuong",
                table: "BenhNhans",
                column: "GiuongMaGiuong",
                principalTable: "Giuongs",
                principalColumn: "MaGiuong",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BenhNhans_Giuongs_GiuongMaGiuong",
                table: "BenhNhans");

            migrationBuilder.DropTable(
                name: "Giuongs");

            migrationBuilder.DropIndex(
                name: "IX_BenhNhans_GiuongMaGiuong",
                table: "BenhNhans");

            migrationBuilder.DropColumn(
                name: "GiuongMaGiuong",
                table: "BenhNhans");

            migrationBuilder.AlterColumn<string>(
                name: "TenKhoa",
                table: "Khoas",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }
    }
}
