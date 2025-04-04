namespace QuanLyBenhVienNoiTru.Models
{
public class Khoa
{
    public int MaKhoa { get; set; }
    public string TenKhoa { get; set; }
    public string MoTa { get; set; }

    // Navigation properties
    public virtual ICollection<BenhNhan> BenhNhans { get; set; }
}
}