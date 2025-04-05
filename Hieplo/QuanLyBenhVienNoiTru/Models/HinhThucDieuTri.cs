namespace QuanLyBenhVienNoiTru.Models
{
    public class HinhThucDieuTri
    {
        public int MaDieuTri { get; set; }
        public required string TenDieuTri { get; set; }
        public decimal ChiPhi { get; set; }
        public required string MoTa { get; set; }
        
        // Khóa ngoại đến bảng Khoa
        public int MaKhoa { get; set; }

        // Navigation properties
        public virtual ICollection<DieuTriBenhNhan> DieuTriBenhNhans { get; set; } = new List<DieuTriBenhNhan>();
        public virtual Khoa Khoa { get; set; }
    }
}
