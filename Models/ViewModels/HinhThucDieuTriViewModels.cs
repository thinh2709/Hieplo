using System.ComponentModel.DataAnnotations;

namespace QuanLyBenhVienNoiTru.Models.ViewModels
{
    public class HinhThucDieuTriViewModel
    {
        public int MaDieuTri { get; set; }
        
        [Required(ErrorMessage = "Tên điều trị không được để trống")]
        [Display(Name = "Tên điều trị")]
        public string TenDieuTri { get; set; }
        
        [Required(ErrorMessage = "Chi phí không được để trống")]
        [Display(Name = "Chi phí")]
        [DataType(DataType.Currency)]
        [Range(0, double.MaxValue, ErrorMessage = "Chi phí phải lớn hơn hoặc bằng 0")]
        public decimal ChiPhi { get; set; }
        
        [Display(Name = "Mô tả")]
        public string MoTa { get; set; }
    }

    public class HinhThucDieuTriListViewModel
    {
        public List<HinhThucDieuTri> HinhThucDieuTris { get; set; }
        public string SearchTerm { get; set; }
        public PaginationInfo PaginationInfo { get; set; }
    }

    public class PaginationInfo
    {
        public int TotalItems { get; set; }
        public int ItemsPerPage { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages => (int)Math.Ceiling((decimal)TotalItems / ItemsPerPage);
    }
}