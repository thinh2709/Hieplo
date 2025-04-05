namespace QuanLyBenhVienNoiTru.Models
{
public class Khoa
{
    public int MaKhoa { get; set; }
    public string TenKhoa { get; set; }
    public string MoTa { get; set; }
    public bool TrangThai { get; set; }

    // Navigation properties
    public virtual ICollection<BenhNhan> BenhNhans { get; set; } = new List<BenhNhan>();
    public virtual ICollection<BacSi> BacSis { get; set; } = new List<BacSi>();
    public virtual ICollection<HinhThucDieuTri> HinhThucDieuTris { get; set; } = new List<HinhThucDieuTri>();
}
}