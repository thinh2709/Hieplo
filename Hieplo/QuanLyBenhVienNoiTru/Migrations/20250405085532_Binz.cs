using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyBenhVienNoiTru.Migrations
{
    /// <inheritdoc />
    public partial class Binz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaKhoa",
                table: "HinhThucDieuTris",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "NoiDung",
                table: "DieuTriBenhNhans",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "KetQua",
                table: "DieuTriBenhNhans",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_HinhThucDieuTris_MaKhoa",
                table: "HinhThucDieuTris",
                column: "MaKhoa");

            migrationBuilder.AddForeignKey(
                name: "FK_HinhThucDieuTris_Khoas_MaKhoa",
                table: "HinhThucDieuTris",
                column: "MaKhoa",
                principalTable: "Khoas",
                principalColumn: "MaKhoa");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HinhThucDieuTris_Khoas_MaKhoa",
                table: "HinhThucDieuTris");

            migrationBuilder.DropIndex(
                name: "IX_HinhThucDieuTris_MaKhoa",
                table: "HinhThucDieuTris");

            migrationBuilder.DropColumn(
                name: "MaKhoa",
                table: "HinhThucDieuTris");

            migrationBuilder.AlterColumn<string>(
                name: "NoiDung",
                table: "DieuTriBenhNhans",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "KetQua",
                table: "DieuTriBenhNhans",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
