namespace QuanLyBenhVienNoiTru.Models
{
    public class LichThamBenh
    {
        public int MaLich { get; set; }
        public int? MaKhach { get; set; }
        public int? MaBenhNhan { get; set; }
        public DateTime? ThoiGianTham { get; set; }
        public string TrangThai { get; set; }

        // Navigation properties
        public virtual KhachThamBenh KhachThamBenh { get; set; }
        public virtual BenhNhan BenhNhan { get; set; }
    }
}