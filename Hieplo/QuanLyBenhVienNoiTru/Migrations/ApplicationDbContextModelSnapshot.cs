﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using QuanLyBenhVienNoiTru.Data;

#nullable disable

namespace QuanLyBenhVienNoiTru.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("QuanLyBenhVienNoiTru.Models.BacSi", b =>
                {
                    b.Property<int>("MaBacSi")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MaBacSi"));

                    b.Property<string>("ChuyenKhoa")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("GioiTinh")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("HoTen")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("MaTaiKhoan")
                        .HasColumnType("int");

                    b.Property<string>("SoDienThoai")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("MaBacSi");

                    b.HasIndex("MaTaiKhoan")
                        .IsUnique()
                        .HasFilter("[MaTaiKhoan] IS NOT NULL");

                    b.ToTable("BacSis");
                });

            modelBuilder.Entity("QuanLyBenhVienNoiTru.Models.BenhNhan", b =>
                {
                    b.Property<int>("MaBenhNhan")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MaBenhNhan"));

                    b.Property<bool>("BaoHiemYTe")
                        .HasColumnType("bit");

                    b.Property<string>("DiaChi")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("GioiTinh")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("HoTen")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("MaKhoa")
                        .HasColumnType("int");

                    b.Property<DateTime>("NgayNhapVien")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("NgaySinh")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("NgayXuatVien")
                        .HasColumnType("datetime2");

                    b.Property<string>("SoDienThoai")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("MaBenhNhan");

                    b.HasIndex("MaKhoa");

                    b.ToTable("BenhNhans");
                });

            modelBuilder.Entity("QuanLyBenhVienNoiTru.Models.ChiPhiDieuTri", b =>
                {
                    b.Property<int>("MaChiPhi")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MaChiPhi"));

                    b.Property<bool>("DaThanhToan")
                        .HasColumnType("bit");

                    b.Property<int?>("MaBenhNhan")
                        .HasColumnType("int");

                    b.Property<DateTime?>("NgayLap")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("TongChiPhi")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("MaChiPhi");

                    b.HasIndex("MaBenhNhan");

                    b.ToTable("ChiPhiDieuTris");
                });

            modelBuilder.Entity("QuanLyBenhVienNoiTru.Models.DieuTriBenhNhan", b =>
                {
                    b.Property<int>("MaDieuTriBenhNhan")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MaDieuTriBenhNhan"));

                    b.Property<string>("KetQua")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("MaBacSi")
                        .HasColumnType("int");

                    b.Property<int?>("MaBenhNhan")
                        .HasColumnType("int");

                    b.Property<int?>("MaDieuTri")
                        .HasColumnType("int");

                    b.Property<DateTime?>("NgayThucHien")
                        .HasColumnType("datetime2");

                    b.HasKey("MaDieuTriBenhNhan");

                    b.HasIndex("MaBacSi");

                    b.HasIndex("MaBenhNhan");

                    b.HasIndex("MaDieuTri");

                    b.ToTable("DieuTriBenhNhans");
                });

            modelBuilder.Entity("QuanLyBenhVienNoiTru.Models.HinhThucDieuTri", b =>
                {
                    b.Property<int>("MaDieuTri")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MaDieuTri"));

                    b.Property<decimal>("ChiPhi")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("MoTa")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TenDieuTri")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("MaDieuTri");

                    b.ToTable("HinhThucDieuTris");
                });

            modelBuilder.Entity("QuanLyBenhVienNoiTru.Models.KhachThamBenh", b =>
                {
                    b.Property<int>("MaKhach")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MaKhach"));

                    b.Property<string>("HoTen")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("MaTaiKhoan")
                        .HasColumnType("int");

                    b.Property<string>("MoiQuanHe")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SoDienThoai")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("MaKhach");

                    b.HasIndex("MaTaiKhoan")
                        .IsUnique()
                        .HasFilter("[MaTaiKhoan] IS NOT NULL");

                    b.ToTable("KhachThamBenhs");
                });

            modelBuilder.Entity("QuanLyBenhVienNoiTru.Models.Khoa", b =>
                {
                    b.Property<int>("MaKhoa")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MaKhoa"));

                    b.Property<string>("MoTa")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TenKhoa")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("MaKhoa");

                    b.ToTable("Khoas");
                });

            modelBuilder.Entity("QuanLyBenhVienNoiTru.Models.LichThamBenh", b =>
                {
                    b.Property<int>("MaLich")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MaLich"));

                    b.Property<int?>("MaBenhNhan")
                        .HasColumnType("int");

                    b.Property<int?>("MaKhach")
                        .HasColumnType("int");

                    b.Property<DateTime?>("ThoiGianTham")
                        .HasColumnType("datetime2");

                    b.Property<string>("TrangThai")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("MaLich");

                    b.HasIndex("MaBenhNhan");

                    b.HasIndex("MaKhach");

                    b.ToTable("LichThamBenhs");
                });

            modelBuilder.Entity("QuanLyBenhVienNoiTru.Models.TaiKhoan", b =>
                {
                    b.Property<int>("MaTaiKhoan")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MaTaiKhoan"));

                    b.Property<string>("MatKhau")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TenDangNhap")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("VaiTro")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("MaTaiKhoan");

                    b.ToTable("TaiKhoans");
                });

            modelBuilder.Entity("QuanLyBenhVienNoiTru.Models.BacSi", b =>
                {
                    b.HasOne("QuanLyBenhVienNoiTru.Models.TaiKhoan", "TaiKhoan")
                        .WithOne("BacSi")
                        .HasForeignKey("QuanLyBenhVienNoiTru.Models.BacSi", "MaTaiKhoan");

                    b.Navigation("TaiKhoan");
                });

            modelBuilder.Entity("QuanLyBenhVienNoiTru.Models.BenhNhan", b =>
                {
                    b.HasOne("QuanLyBenhVienNoiTru.Models.Khoa", "Khoa")
                        .WithMany("BenhNhans")
                        .HasForeignKey("MaKhoa")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Khoa");
                });

            modelBuilder.Entity("QuanLyBenhVienNoiTru.Models.ChiPhiDieuTri", b =>
                {
                    b.HasOne("QuanLyBenhVienNoiTru.Models.BenhNhan", "BenhNhan")
                        .WithMany("ChiPhiDieuTris")
                        .HasForeignKey("MaBenhNhan");

                    b.Navigation("BenhNhan");
                });

            modelBuilder.Entity("QuanLyBenhVienNoiTru.Models.DieuTriBenhNhan", b =>
                {
                    b.HasOne("QuanLyBenhVienNoiTru.Models.BacSi", "BacSi")
                        .WithMany("DieuTriBenhNhans")
                        .HasForeignKey("MaBacSi");

                    b.HasOne("QuanLyBenhVienNoiTru.Models.BenhNhan", "BenhNhan")
                        .WithMany("DieuTriBenhNhans")
                        .HasForeignKey("MaBenhNhan");

                    b.HasOne("QuanLyBenhVienNoiTru.Models.HinhThucDieuTri", "HinhThucDieuTri")
                        .WithMany("DieuTriBenhNhans")
                        .HasForeignKey("MaDieuTri");

                    b.Navigation("BacSi");

                    b.Navigation("BenhNhan");

                    b.Navigation("HinhThucDieuTri");
                });

            modelBuilder.Entity("QuanLyBenhVienNoiTru.Models.KhachThamBenh", b =>
                {
                    b.HasOne("QuanLyBenhVienNoiTru.Models.TaiKhoan", "TaiKhoan")
                        .WithOne("KhachThamBenh")
                        .HasForeignKey("QuanLyBenhVienNoiTru.Models.KhachThamBenh", "MaTaiKhoan");

                    b.Navigation("TaiKhoan");
                });

            modelBuilder.Entity("QuanLyBenhVienNoiTru.Models.LichThamBenh", b =>
                {
                    b.HasOne("QuanLyBenhVienNoiTru.Models.BenhNhan", "BenhNhan")
                        .WithMany("LichThamBenhs")
                        .HasForeignKey("MaBenhNhan");

                    b.HasOne("QuanLyBenhVienNoiTru.Models.KhachThamBenh", "KhachThamBenh")
                        .WithMany("LichThamBenhs")
                        .HasForeignKey("MaKhach");

                    b.Navigation("BenhNhan");

                    b.Navigation("KhachThamBenh");
                });

            modelBuilder.Entity("QuanLyBenhVienNoiTru.Models.BacSi", b =>
                {
                    b.Navigation("DieuTriBenhNhans");
                });

            modelBuilder.Entity("QuanLyBenhVienNoiTru.Models.BenhNhan", b =>
                {
                    b.Navigation("ChiPhiDieuTris");

                    b.Navigation("DieuTriBenhNhans");

                    b.Navigation("LichThamBenhs");
                });

            modelBuilder.Entity("QuanLyBenhVienNoiTru.Models.HinhThucDieuTri", b =>
                {
                    b.Navigation("DieuTriBenhNhans");
                });

            modelBuilder.Entity("QuanLyBenhVienNoiTru.Models.KhachThamBenh", b =>
                {
                    b.Navigation("LichThamBenhs");
                });

            modelBuilder.Entity("QuanLyBenhVienNoiTru.Models.Khoa", b =>
                {
                    b.Navigation("BenhNhans");
                });

            modelBuilder.Entity("QuanLyBenhVienNoiTru.Models.TaiKhoan", b =>
                {
                    b.Navigation("BacSi")
                        .IsRequired();

                    b.Navigation("KhachThamBenh")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
