namespace QuanLyBenhVienNoiTru.Models
{
    public class KhachThamBenh
    {
        public int MaKhach { get; set; }
        public string HoTen { get; set; }
        public int? MaTaiKhoan { get; set; }
        public string SoDienThoai { get; set; }
        public string MoiQuanHe { get; set; }

        // Navigation properties
        public virtual TaiKhoan TaiKhoan { get; set; }
        public virtual ICollection<LichThamBenh> LichThamBenhs { get; set; }
    }
}