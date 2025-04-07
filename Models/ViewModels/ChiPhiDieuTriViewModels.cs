using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyBenhVienNoiTru.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLyBenhVienNoiTru.Models.ViewModels
{
    public class ChiPhiDieuTriListViewModel
    {
        public List<ChiPhiDieuTri> ChiPhiDieuTris { get; set; }
        public string SearchTerm { get; set; }
        public bool? TrangThaiThanhToan { get; set; }
        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }
    }

    public class ChiPhiDieuTriCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn bệnh nhân")]
        [Display(Name = "Bệnh nhân")]
        public int? MaBenhNhan { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tổng chi phí")]
        [Range(0, double.MaxValue, ErrorMessage = "Chi phí phải là số dương")]
        [Display(Name = "Tổng chi phí")]
        [DataType(DataType.Currency)]
        public decimal TongChiPhi { get; set; }

        [Display(Name = "Đã thanh toán")]
        public bool DaThanhToan { get; set; }

        [Display(Name = "Ngày lập")]
        [DataType(DataType.Date)]
        public DateTime? NgayLap { get; set; } = DateTime.Now;

        public SelectList BenhNhanSelectList { get; set; }

        // Thông tin thêm về bệnh nhân (nếu đã biết mã bệnh nhân)
        public BenhNhan BenhNhan { get; set; }
    }

    public class ChiPhiDieuTriDetailsViewModel
    {
        public ChiPhiDieuTri ChiPhiDieuTri { get; set; }
        public List<DieuTriBenhNhan> CacDieuTri { get; set; }
        
        // Thông tin tổng hợp
        public decimal TongChiPhiDieuTri { get; set; }
        public decimal TongChiPhiDieuTriCoBaoHiem { get; set; }
        public decimal SoTienDaThanhToan { get; set; }
        public decimal SoTienConLai { get; set; }
    }

    public class ChiPhiDieuTriThanhToanViewModel
    {
        public int MaChiPhi { get; set; }
        
        [Display(Name = "Bệnh nhân")]
        public string TenBenhNhan { get; set; }
        
        [Display(Name = "Tổng chi phí")]
        [DataType(DataType.Currency)]
        public decimal TongChiPhi { get; set; }
        
        [Display(Name = "Có bảo hiểm y tế")]
        public bool CoBaoHiem { get; set; }
        
        [Display(Name = "Chi phí sau bảo hiểm")]
        [DataType(DataType.Currency)]
        public decimal ChiPhiSauBaoHiem { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập số tiền thanh toán")]
        [Range(0, double.MaxValue, ErrorMessage = "Số tiền thanh toán phải lớn hơn 0")]
        [Display(Name = "Số tiền thanh toán")]
        [DataType(DataType.Currency)]
        public decimal SoTienThanhToan { get; set; }
        
        [Display(Name = "Phương thức thanh toán")]
        public string PhuongThucThanhToan { get; set; }
        
        public List<SelectListItem> PhuongThucThanhToanOptions { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "Tiền mặt", Text = "Tiền mặt" },
            new SelectListItem { Value = "Thẻ ngân hàng", Text = "Thẻ ngân hàng" },
            new SelectListItem { Value = "Chuyển khoản", Text = "Chuyển khoản" }
        };
    }

    public class ChiPhiDieuTriReportViewModel
    {
        public DateTime TuNgay { get; set; }
        public DateTime DenNgay { get; set; }
        
        public int TongSoHoaDon { get; set; }
        public int SoHoaDonDaThanhToan { get; set; }
        public int SoHoaDonChuaThanhToan { get; set; }
        
        public decimal TongDoanhThu { get; set; }
        public decimal TongDoanhThuDaThanhToan { get; set; }
        public decimal TongDoanhThuChuaThanhToan { get; set; }
        
        public Dictionary<string, decimal> DoanhThuTheoKhoa { get; set; }
        public Dictionary<string, decimal> DoanhThuTheoThang { get; set; }
        
        public List<ChiPhiDieuTri> DanhSachChiPhi { get; set; }
    }
}