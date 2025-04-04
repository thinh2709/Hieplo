using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QuanLyBenhVienNoiTru.Models;

namespace QuanLyBenhVienNoiTru.Models.ViewModels
{
    public class DieuTriBenhNhanListViewModel
    {
        public int MaDieuTriBenhNhan { get; set; }
        
        [Display(Name = "Bệnh nhân")]
        public string TenBenhNhan { get; set; }
        
        [Display(Name = "Hình thức điều trị")]
        public string TenDieuTri { get; set; }
        
        [Display(Name = "Bác sĩ")]
        public string TenBacSi { get; set; }
        
        [Display(Name = "Ngày thực hiện")]
        [DataType(DataType.Date)]
        public DateTime? NgayThucHien { get; set; }
        
        [Display(Name = "Kết quả")]
        public string KetQua { get; set; }
        
        [Display(Name = "Chi phí")]
        [DataType(DataType.Currency)]
        public decimal ChiPhi { get; set; }
    }
    
    public class DieuTriBenhNhanCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn bệnh nhân")]
        [Display(Name = "Bệnh nhân")]
        public int? MaBenhNhan { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn hình thức điều trị")]
        [Display(Name = "Hình thức điều trị")]
        public int? MaDieuTri { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn bác sĩ")]
        [Display(Name = "Bác sĩ")]
        public int? MaBacSi { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn ngày thực hiện")]
        [Display(Name = "Ngày thực hiện")]
        [DataType(DataType.Date)]
        public DateTime? NgayThucHien { get; set; }
        
        [Display(Name = "Kết quả")]
        public string KetQua { get; set; }
    }
    
    public class DieuTriBenhNhanEditViewModel
    {
        public int MaDieuTriBenhNhan { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn bệnh nhân")]
        [Display(Name = "Bệnh nhân")]
        public int? MaBenhNhan { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn hình thức điều trị")]
        [Display(Name = "Hình thức điều trị")]
        public int? MaDieuTri { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn bác sĩ")]
        [Display(Name = "Bác sĩ")]
        public int? MaBacSi { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn ngày thực hiện")]
        [Display(Name = "Ngày thực hiện")]
        [DataType(DataType.Date)]
        public DateTime? NgayThucHien { get; set; }
        
        [Display(Name = "Kết quả")]
        public string KetQua { get; set; }
    }
    
    public class DieuTriBenhNhanDetailsViewModel
    {
        public int MaDieuTriBenhNhan { get; set; }
        
        [Display(Name = "Bệnh nhân")]
        public string TenBenhNhan { get; set; }
        
        [Display(Name = "Hình thức điều trị")]
        public string TenDieuTri { get; set; }
        
        [Display(Name = "Bác sĩ")]
        public string TenBacSi { get; set; }
        
        [Display(Name = "Ngày thực hiện")]
        [DataType(DataType.Date)]
        public DateTime? NgayThucHien { get; set; }
        
        [Display(Name = "Kết quả")]
        public string KetQua { get; set; }
        
        [Display(Name = "Chi phí")]
        [DataType(DataType.Currency)]
        public decimal ChiPhi { get; set; }
        
        [Display(Name = "Đã thanh toán")]
        public bool DaThanhToan { get; set; }
    }
    
    public class BenhNhanDieuTriViewModel
    {
        public int MaBenhNhan { get; set; }
        
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; }
        
        [Display(Name = "Khoa")]
        public string TenKhoa { get; set; }
        
        [Display(Name = "Ngày nhập viện")]
        [DataType(DataType.Date)]
        public DateTime? NgayNhapVien { get; set; }
        
        [Display(Name = "Tổng chi phí")]
        [DataType(DataType.Currency)]
        public decimal TongChiPhi { get; set; }
        
        [Display(Name = "Đã thanh toán")]
        public bool DaThanhToan { get; set; }
        
        [Display(Name = "Danh sách điều trị")]
        public List<DieuTriBenhNhanListViewModel> DanhSachDieuTri { get; set; }
    }
}