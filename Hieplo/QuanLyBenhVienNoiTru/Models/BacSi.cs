namespace QuanLyBenhVienNoiTru.Models
{
    public class BacSi
    {
        public int MaBacSi { get; set; }
        public string HoTen { get; set; }
        public int? MaTaiKhoan { get; set; }
        public string ChuyenKhoa { get; set; }
        public string SoDienThoai { get; set; }
        public string GioiTinh { get; set; }

        // Navigation properties
        public virtual TaiKhoan TaiKhoan { get; set; }
        public virtual ICollection<DieuTriBenhNhan> DieuTriBenhNhans { get; set; }
    }
}