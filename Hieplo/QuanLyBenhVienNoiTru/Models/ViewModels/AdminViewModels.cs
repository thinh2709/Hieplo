using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QuanLyBenhVienNoiTru.Models;

namespace QuanLyBenhVienNoiTru.Models.ViewModels
{
    // Dashboard statistics view model
    public class DashboardViewModel
    {
        public int TongBenhNhan { get; set; }
        public int BenhNhanDangDieuTri { get; set; }
        public int TongBacSi { get; set; }
        public int TongKhoa { get; set; }
    }
    
    // User management view model
    public class UserManagementViewModel
    {
        public List<TaiKhoan> Users { get; set; }
    }
    
    // User edit view model
    public class EditUserViewModel
    {
        public int MaTaiKhoan { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; }
        
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        [Display(Name = "Vai trò")]
        public string VaiTro { get; set; }
        
        // List of available roles for dropdown selection
        public List<string> AvailableRoles => new List<string> { "Admin", "Bác sĩ", "Khách" };
    }
    
    // User creation view model
    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [Display(Name = "Mật khẩu")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; }
        
        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [Display(Name = "Xác nhận mật khẩu")]
        [DataType(DataType.Password)]
        [Compare("MatKhau", ErrorMessage = "Mật khẩu không khớp.")]
        public string XacNhanMatKhau { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        [Display(Name = "Vai trò")]
        public string VaiTro { get; set; }
        
        // List of available roles for dropdown selection
        public List<string> AvailableRoles => new List<string> { "Admin", "Bác sĩ", "Khách" };
    }
    
    // Doctor assignment view model for when creating users with doctor role
    public class DoctorAssignmentViewModel
    {
        public int MaTaiKhoan { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập họ tên bác sĩ")]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; }
        
        [Display(Name = "Chuyên khoa")]
        public string ChuyenKhoa { get; set; }
        
        [Display(Name = "Số điện thoại")]
        [RegularExpression(@"^\d{10,15}$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string SoDienThoai { get; set; }
        
        [Display(Name = "Khoa")]
        [Required(ErrorMessage = "Vui lòng chọn khoa")]
        public int MaKhoa { get; set; }
        
        // For dropdown list
        public List<Khoa> DanhSachKhoa { get; set; }
    }
    
    // Visitor assignment view model for when creating users with visitor role
    public class VisitorAssignmentViewModel
    {
        public int MaTaiKhoan { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập họ tên khách")]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; }
        
        [Display(Name = "Số điện thoại")]
        [RegularExpression(@"^\d{10,15}$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string SoDienThoai { get; set; }
        
        [Display(Name = "Mối quan hệ")]
        public string MoiQuanHe { get; set; }
    }
    
    // System statistics view model for reports
    public class SystemStatisticsViewModel
    {
        public int TongBenhNhan { get; set; }
        public int BenhNhanDangDieuTri { get; set; }
        public int TongBacSi { get; set; }
        public int TongKhoa { get; set; }
        public int TongLichThamBenh { get; set; }
        public int LichThamBenhChoDuyet { get; set; }
        public decimal TongDoanhThu { get; set; }
        public decimal DoanhThuChuaThanhToan { get; set; }
    }
    
    // Department management summary view model
    public class DepartmentSummaryViewModel
    {
        public List<DepartmentStatsViewModel> Departments { get; set; }
    }
    
    public class DepartmentStatsViewModel
    {
        public int MaKhoa { get; set; }
        public string TenKhoa { get; set; }
        public int SoBenhNhan { get; set; }
        public int SoBacSi { get; set; }
    }
    
    // Patient statistics view model for reports
    public class PatientStatisticsViewModel
    {
        public int TongBenhNhan { get; set; }
        public int BenhNhanNam { get; set; }
        public int BenhNhanNu { get; set; }
        public int BenhNhanKhac { get; set; }
        public int BenhNhanCoBaoHiem { get; set; }
        public decimal TyLeBaoHiem => TongBenhNhan > 0 ? (decimal)BenhNhanCoBaoHiem / TongBenhNhan * 100 : 0;
        public List<MonthlyPatientCount> ThongKeBenhNhanTheoThang { get; set; }
    }
    
    public class MonthlyPatientCount
    {
        public string Thang { get; set; }
        public int SoBenhNhanNhapVien { get; set; }
        public int SoBenhNhanXuatVien { get; set; }
    }
}