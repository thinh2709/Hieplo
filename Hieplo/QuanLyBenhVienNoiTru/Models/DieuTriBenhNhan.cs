namespace QuanLyBenhVienNoiTru.Models
{
    public class DieuTriBenhNhan
    {
        public int MaDieuTriBenhNhan { get; set; }
        public int? MaBenhNhan { get; set; }
        public int? MaDieuTri { get; set; }
        public int? MaBacSi { get; set; }
        public DateTime? NgayThucHien { get; set; }
        public string KetQua { get; set; }

        // Navigation properties
        public virtual BenhNhan BenhNhan { get; set; }
        public virtual HinhThucDieuTri HinhThucDieuTri { get; set; }
        public virtual BacSi BacSi { get; set; }
    }
}