namespace QuanLyBenhVienNoiTru.Models
{
    public class HinhThucDieuTri
    {
        public int MaDieuTri { get; set; }
        public string TenDieuTri { get; set; }
        public decimal ChiPhi { get; set; }
        public string MoTa { get; set; }

        // Navigation properties
        public virtual ICollection<DieuTriBenhNhan> DieuTriBenhNhans { get; set; }
    }
}
