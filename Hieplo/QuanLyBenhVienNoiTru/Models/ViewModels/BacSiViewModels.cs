using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QuanLyBenhVienNoiTru.Models;

namespace QuanLyBenhVienNoiTru.Models.ViewModels
{
    public class BacSiProfileViewModel
    {
        public int MaBacSi { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập chuyên khoa")]
        [Display(Name = "Chuyên khoa")]
        public string ChuyenKhoa { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; }
    }
    
    public class BacSiDashboardViewModel
    {
        public int TotalPatients { get; set; }
        public int ActivePatients { get; set; }
        public int DischargedToday { get; set; }
        public int TreatmentsToday { get; set; }
        public List<BenhNhan> RecentPatients { get; set; }
        public List<DieuTriBenhNhan> RecentTreatments { get; set; }
    }
    
    public class DieuTriDetailViewModel
    {
        public DieuTriBenhNhan DieuTri { get; set; }
        public BenhNhan BenhNhan { get; set; }
        public HinhThucDieuTri HinhThucDieuTri { get; set; }
        public BacSi BacSi { get; set; }
        public decimal ChiPhi { get; set; }
    }
    
    public class BenhNhanSummaryViewModel
    {
        public BenhNhan BenhNhan { get; set; }
        public List<DieuTriBenhNhan> DieuTriHistory { get; set; }
        public List<ChiPhiDieuTri> ChiPhi { get; set; }
        public decimal TongChiPhi { get; set; }
        public decimal ChuaThanhToan { get; set; }
    }
}