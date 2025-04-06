using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyBenhVienNoiTru.Migrations
{
    /// <inheritdoc />
    public partial class Bin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Khoas",
                columns: table => new
                {
                    MaKhoa = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenKhoa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Khoas", x => x.MaKhoa);
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoans",
                columns: table => new
                {
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDangNhap = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MatKhau = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VaiTro = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoans", x => x.MaTaiKhoan);
                });

            migrationBuilder.CreateTable(
                name: "BacSis",
                columns: table => new
                {
                    MaBacSi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: true),
                    ChuyenKhoa = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GioiTinh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DiaChi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NgayVaoLam = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayNghiViec = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false),
                    MaKhoa = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BacSis", x => x.MaBacSi);
                    table.ForeignKey(
                        name: "FK_BacSis_Khoas_MaKhoa",
                        column: x => x.MaKhoa,
                        principalTable: "Khoas",
                        principalColumn: "MaKhoa",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BacSis_TaiKhoans_MaTaiKhoan",
                        column: x => x.MaTaiKhoan,
                        principalTable: "TaiKhoans",
                        principalColumn: "MaTaiKhoan");
                });

            migrationBuilder.CreateTable(
                name: "KhachThamBenhs",
                columns: table => new
                {
                    MaKhach = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: true),
                    SoDienThoai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DiaChi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MoiQuanHe = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhachThamBenhs", x => x.MaKhach);
                    table.ForeignKey(
                        name: "FK_KhachThamBenhs_TaiKhoans_MaTaiKhoan",
                        column: x => x.MaTaiKhoan,
                        principalTable: "TaiKhoans",
                        principalColumn: "MaTaiKhoan");
                });

            migrationBuilder.CreateTable(
                name: "BenhNhans",
                columns: table => new
                {
                    MaBenhNhan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoTen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgaySinh = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GioiTinh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiaChi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BaoHiemYTe = table.Column<bool>(type: "bit", nullable: false),
                    NgayNhapVien = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayXuatVien = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MaKhoa = table.Column<int>(type: "int", nullable: false),
                    MaBacSi = table.Column<int>(type: "int", nullable: true),
                    ChanDoan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false),
                    BacSiMaBacSi = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BenhNhans", x => x.MaBenhNhan);
                    table.ForeignKey(
                        name: "FK_BenhNhans_BacSis_BacSiMaBacSi",
                        column: x => x.BacSiMaBacSi,
                        principalTable: "BacSis",
                        principalColumn: "MaBacSi");
                    table.ForeignKey(
                        name: "FK_BenhNhans_BacSis_MaBacSi",
                        column: x => x.MaBacSi,
                        principalTable: "BacSis",
                        principalColumn: "MaBacSi");
                    table.ForeignKey(
                        name: "FK_BenhNhans_Khoas_MaKhoa",
                        column: x => x.MaKhoa,
                        principalTable: "Khoas",
                        principalColumn: "MaKhoa");
                });

            migrationBuilder.CreateTable(
                name: "ChiPhiDieuTris",
                columns: table => new
                {
                    MaChiPhi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaBenhNhan = table.Column<int>(type: "int", nullable: true),
                    TongChiPhi = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DaThanhToan = table.Column<bool>(type: "bit", nullable: false),
                    NgayLap = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiPhiDieuTris", x => x.MaChiPhi);
                    table.ForeignKey(
                        name: "FK_ChiPhiDieuTris_BenhNhans_MaBenhNhan",
                        column: x => x.MaBenhNhan,
                        principalTable: "BenhNhans",
                        principalColumn: "MaBenhNhan",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HinhThucDieuTris",
                columns: table => new
                {
                    MaDieuTri = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDieuTri = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChiPhi = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BacSiMaBacSi = table.Column<int>(type: "int", nullable: true),
                    BenhNhanMaBenhNhan = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HinhThucDieuTris", x => x.MaDieuTri);
                    table.ForeignKey(
                        name: "FK_HinhThucDieuTris_BacSis_BacSiMaBacSi",
                        column: x => x.BacSiMaBacSi,
                        principalTable: "BacSis",
                        principalColumn: "MaBacSi");
                    table.ForeignKey(
                        name: "FK_HinhThucDieuTris_BenhNhans_BenhNhanMaBenhNhan",
                        column: x => x.BenhNhanMaBenhNhan,
                        principalTable: "BenhNhans",
                        principalColumn: "MaBenhNhan");
                });

            migrationBuilder.CreateTable(
                name: "LichThamBenhs",
                columns: table => new
                {
                    MaLich = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaKhach = table.Column<int>(type: "int", nullable: true),
                    MaBenhNhan = table.Column<int>(type: "int", nullable: true),
                    ThoiGianTham = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichThamBenhs", x => x.MaLich);
                    table.ForeignKey(
                        name: "FK_LichThamBenhs_BenhNhans_MaBenhNhan",
                        column: x => x.MaBenhNhan,
                        principalTable: "BenhNhans",
                        principalColumn: "MaBenhNhan",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LichThamBenhs_KhachThamBenhs_MaKhach",
                        column: x => x.MaKhach,
                        principalTable: "KhachThamBenhs",
                        principalColumn: "MaKhach");
                });

            migrationBuilder.CreateTable(
                name: "DieuTriBenhNhans",
                columns: table => new
                {
                    MaDieuTriBenhNhan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaBenhNhan = table.Column<int>(type: "int", nullable: true),
                    MaDieuTri = table.Column<int>(type: "int", nullable: true),
                    MaBacSi = table.Column<int>(type: "int", nullable: true),
                    NgayThucHien = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NgayKetThuc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    KetQua = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DieuTriBenhNhans", x => x.MaDieuTriBenhNhan);
                    table.ForeignKey(
                        name: "FK_DieuTriBenhNhans_BacSis_MaBacSi",
                        column: x => x.MaBacSi,
                        principalTable: "BacSis",
                        principalColumn: "MaBacSi");
                    table.ForeignKey(
                        name: "FK_DieuTriBenhNhans_BenhNhans_MaBenhNhan",
                        column: x => x.MaBenhNhan,
                        principalTable: "BenhNhans",
                        principalColumn: "MaBenhNhan",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DieuTriBenhNhans_HinhThucDieuTris_MaDieuTri",
                        column: x => x.MaDieuTri,
                        principalTable: "HinhThucDieuTris",
                        principalColumn: "MaDieuTri");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BacSis_MaKhoa",
                table: "BacSis",
                column: "MaKhoa");

            migrationBuilder.CreateIndex(
                name: "IX_BacSis_MaTaiKhoan",
                table: "BacSis",
                column: "MaTaiKhoan",
                unique: true,
                filter: "[MaTaiKhoan] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BenhNhans_BacSiMaBacSi",
                table: "BenhNhans",
                column: "BacSiMaBacSi");

            migrationBuilder.CreateIndex(
                name: "IX_BenhNhans_MaBacSi",
                table: "BenhNhans",
                column: "MaBacSi");

            migrationBuilder.CreateIndex(
                name: "IX_BenhNhans_MaKhoa",
                table: "BenhNhans",
                column: "MaKhoa");

            migrationBuilder.CreateIndex(
                name: "IX_ChiPhiDieuTris_MaBenhNhan",
                table: "ChiPhiDieuTris",
                column: "MaBenhNhan");

            migrationBuilder.CreateIndex(
                name: "IX_DieuTriBenhNhans_MaBacSi",
                table: "DieuTriBenhNhans",
                column: "MaBacSi");

            migrationBuilder.CreateIndex(
                name: "IX_DieuTriBenhNhans_MaBenhNhan",
                table: "DieuTriBenhNhans",
                column: "MaBenhNhan");

            migrationBuilder.CreateIndex(
                name: "IX_DieuTriBenhNhans_MaDieuTri",
                table: "DieuTriBenhNhans",
                column: "MaDieuTri");

            migrationBuilder.CreateIndex(
                name: "IX_HinhThucDieuTris_BacSiMaBacSi",
                table: "HinhThucDieuTris",
                column: "BacSiMaBacSi");

            migrationBuilder.CreateIndex(
                name: "IX_HinhThucDieuTris_BenhNhanMaBenhNhan",
                table: "HinhThucDieuTris",
                column: "BenhNhanMaBenhNhan");

            migrationBuilder.CreateIndex(
                name: "IX_KhachThamBenhs_MaTaiKhoan",
                table: "KhachThamBenhs",
                column: "MaTaiKhoan",
                unique: true,
                filter: "[MaTaiKhoan] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LichThamBenhs_MaBenhNhan",
                table: "LichThamBenhs",
                column: "MaBenhNhan");

            migrationBuilder.CreateIndex(
                name: "IX_LichThamBenhs_MaKhach",
                table: "LichThamBenhs",
                column: "MaKhach");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiPhiDieuTris");

            migrationBuilder.DropTable(
                name: "DieuTriBenhNhans");

            migrationBuilder.DropTable(
                name: "LichThamBenhs");

            migrationBuilder.DropTable(
                name: "HinhThucDieuTris");

            migrationBuilder.DropTable(
                name: "KhachThamBenhs");

            migrationBuilder.DropTable(
                name: "BenhNhans");

            migrationBuilder.DropTable(
                name: "BacSis");

            migrationBuilder.DropTable(
                name: "Khoas");

            migrationBuilder.DropTable(
                name: "TaiKhoans");
        }
    }
}
