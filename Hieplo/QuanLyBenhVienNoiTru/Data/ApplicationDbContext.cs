using Microsoft.EntityFrameworkCore;
using QuanLyBenhVienNoiTru.Models;

namespace QuanLyBenhVienNoiTru.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<BacSi> BacSis { get; set; }
        public DbSet<BenhNhan> BenhNhans { get; set; }
        public DbSet<ChiPhiDieuTri> ChiPhiDieuTris { get; set; }
        public DbSet<DieuTriBenhNhan> DieuTriBenhNhans { get; set; }
        public DbSet<HinhThucDieuTri> HinhThucDieuTris { get; set; }
        public DbSet<KhachThamBenh> KhachThamBenhs { get; set; }
        public DbSet<Khoa> Khoas { get; set; }
        public DbSet<LichThamBenh> LichThamBenhs { get; set; }
        public DbSet<TaiKhoan> TaiKhoans { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure relationships

            // Chỉ định khóa chính cho các bảng
            modelBuilder.Entity<BacSi>().HasKey(b => b.MaBacSi);
            modelBuilder.Entity<BenhNhan>().HasKey(b => b.MaBenhNhan);
            modelBuilder.Entity<ChiPhiDieuTri>().HasKey(c => c.MaChiPhi);
            modelBuilder.Entity<DieuTriBenhNhan>().HasKey(d => d.MaDieuTriBenhNhan);
            modelBuilder.Entity<HinhThucDieuTri>().HasKey(h => h.MaDieuTri);
            modelBuilder.Entity<KhachThamBenh>().HasKey(k => k.MaKhach);
            modelBuilder.Entity<Khoa>().HasKey(k => k.MaKhoa);
            modelBuilder.Entity<LichThamBenh>().HasKey(l => l.MaLich);
            modelBuilder.Entity<TaiKhoan>().HasKey(t => t.MaTaiKhoan);

            // Quan hệ giữa BacSi và TaiKhoan
            modelBuilder.Entity<BacSi>()
                .HasOne(b => b.TaiKhoan)
                .WithOne(t => t.BacSi)
                .HasForeignKey<BacSi>(b => b.MaTaiKhoan);

            // Quan hệ giữa BenhNhan và Khoa
            modelBuilder.Entity<BenhNhan>()
                .HasOne(b => b.Khoa)
                .WithMany(k => k.BenhNhans)
                .HasForeignKey(b => b.MaKhoa);

            // Quan hệ giữa ChiPhiDieuTri và BenhNhan
            modelBuilder.Entity<ChiPhiDieuTri>()
                .HasOne(c => c.BenhNhan)
                .WithMany(b => b.ChiPhiDieuTris)
                .HasForeignKey(c => c.MaBenhNhan);

            // Quan hệ giữa DieuTriBenhNhan và BenhNhan
            modelBuilder.Entity<DieuTriBenhNhan>()
                .HasOne(d => d.BenhNhan)
                .WithMany(b => b.DieuTriBenhNhans)
                .HasForeignKey(d => d.MaBenhNhan);

            // Quan hệ giữa DieuTriBenhNhan và HinhThucDieuTri
            modelBuilder.Entity<DieuTriBenhNhan>()
                .HasOne(d => d.HinhThucDieuTri)
                .WithMany(h => h.DieuTriBenhNhans)
                .HasForeignKey(d => d.MaDieuTri);

            // Quan hệ giữa DieuTriBenhNhan và BacSi
            modelBuilder.Entity<DieuTriBenhNhan>()
                .HasOne(d => d.BacSi)
                .WithMany(b => b.DieuTriBenhNhans)
                .HasForeignKey(d => d.MaBacSi);

            // Quan hệ giữa KhachThamBenh và TaiKhoan
            modelBuilder.Entity<KhachThamBenh>()
                .HasOne(k => k.TaiKhoan)
                .WithOne(t => t.KhachThamBenh)
                .HasForeignKey<KhachThamBenh>(k => k.MaTaiKhoan);

            // Quan hệ giữa LichThamBenh và KhachThamBenh
            modelBuilder.Entity<LichThamBenh>()
                .HasOne(l => l.KhachThamBenh)
                .WithMany(k => k.LichThamBenhs)
                .HasForeignKey(l => l.MaKhach);

            // Quan hệ giữa LichThamBenh và BenhNhan
            modelBuilder.Entity<LichThamBenh>()
                .HasOne(l => l.BenhNhan)
                .WithMany(b => b.LichThamBenhs)
                .HasForeignKey(l => l.MaBenhNhan);
        }
    }
}
