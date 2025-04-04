namespace QuanLyBenhVienNoiTru.Models
{
    public class BenhNhan
        {
            public int MaBenhNhan { get; set; }
            public string HoTen { get; set; }
            public DateTime? NgaySinh { get; set; }
            public string GioiTinh { get; set; }
            public string SoDienThoai { get; set; }
            public string DiaChi { get; set; }
            public bool BaoHiemYTe { get; set; }
            public DateTime NgayNhapVien { get; set; }
            public DateTime? NgayXuatVien { get; set; }
            public int MaKhoa { get; set; }

            // Navigation properties
            public virtual Khoa Khoa { get; set; }
            public virtual ICollection<DieuTriBenhNhan> DieuTriBenhNhans { get; set; }
            public virtual ICollection<LichThamBenh> LichThamBenhs { get; set; }
            public virtual ICollection<ChiPhiDieuTri> ChiPhiDieuTris { get; set; }
        }
}