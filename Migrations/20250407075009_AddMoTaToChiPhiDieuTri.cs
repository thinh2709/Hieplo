using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyBenhVienNoiTru.Migrations
{
    /// <inheritdoc />
    public partial class AddMoTaToChiPhiDieuTri : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BenhNhans_Giuongs_GiuongMaGiuong",
                table: "BenhNhans");

            migrationBuilder.DropIndex(
                name: "IX_BenhNhans_GiuongMaGiuong",
                table: "BenhNhans");

            migrationBuilder.AlterColumn<string>(
                name: "TenDieuTri",
                table: "HinhThucDieuTris",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "MoTa",
                table: "HinhThucDieuTris",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "MoTa",
                table: "ChiPhiDieuTris",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MoTa",
                table: "ChiPhiDieuTris");

            migrationBuilder.AlterColumn<string>(
                name: "TenDieuTri",
                table: "HinhThucDieuTris",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "MoTa",
                table: "HinhThucDieuTris",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.CreateIndex(
                name: "IX_BenhNhans_GiuongMaGiuong",
                table: "BenhNhans",
                column: "GiuongMaGiuong");

            migrationBuilder.AddForeignKey(
                name: "FK_BenhNhans_Giuongs_GiuongMaGiuong",
                table: "BenhNhans",
                column: "GiuongMaGiuong",
                principalTable: "Giuongs",
                principalColumn: "MaGiuong",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
