using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyBenhVienNoiTru.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLyBenhVienNoiTru.Models.ViewModels
{
    public class BenhNhanListViewModel
    {
        public List<BenhNhan> BenhNhans { get; set; }
        public string SearchTerm { get; set; }
        public int? KhoaFilter { get; set; }
        public bool ShowOnlyInpatients { get; set; }
        public SelectList KhoaSelectList { get; set; }
    }

    public class BenhNhanCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; }

        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime? NgaySinh { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giới tính")]
        [Display(Name = "Giới tính")]
        public string GioiTinh { get; set; }

        [Display(Name = "Số điện thoại")]
        [RegularExpression(@"^[0-9]{10,11}$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string SoDienThoai { get; set; }

        [Display(Name = "Địa chỉ")]
        public string DiaChi { get; set; }

        [Display(Name = "Có bảo hiểm y tế")]
        public bool BaoHiemYTe { get; set; }

        [Required(ErrorMessage = "Ngày nhập viện là bắt buộc")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày nhập viện")]
        public DateTime NgayNhapVien { get; set; } = DateTime.Now;

        [DataType(DataType.Date)]
        [Display(Name = "Ngày xuất viện")]
        public DateTime? NgayXuatVien { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn khoa")]
        [Display(Name = "Khoa")]
        public int MaKhoa { get; set; }
        
        public SelectList KhoaSelectList { get; set; }
        public List<SelectListItem> GioiTinhOptions { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "Nam", Text = "Nam" },
            new SelectListItem { Value = "Nữ", Text = "Nữ" },
            new SelectListItem { Value = "Khác", Text = "Khác" }
        };
    }

    public class BenhNhanEditViewModel
    {
        public int MaBenhNhan { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; }

        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime? NgaySinh { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giới tính")]
        [Display(Name = "Giới tính")]
        public string GioiTinh { get; set; }

        [Display(Name = "Số điện thoại")]
        [RegularExpression(@"^[0-9]{10,11}$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string SoDienThoai { get; set; }

        [Display(Name = "Địa chỉ")]
        public string DiaChi { get; set; }

        [Display(Name = "Có bảo hiểm y tế")]
        public bool BaoHiemYTe { get; set; }

        [Required(ErrorMessage = "Ngày nhập viện là bắt buộc")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày nhập viện")]
        public DateTime NgayNhapVien { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Ngày xuất viện")]
        public DateTime? NgayXuatVien { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn khoa")]
        [Display(Name = "Khoa")]
        public int MaKhoa { get; set; }
        
        public SelectList KhoaSelectList { get; set; }
        public List<SelectListItem> GioiTinhOptions { get; set; }
    }

    public class BenhNhanDetailsViewModel
    {
        public BenhNhan BenhNhan { get; set; }
        public List<DieuTriBenhNhan> DieuTris { get; set; }
        public List<ChiPhiDieuTri> ChiPhis { get; set; }
        public List<LichThamBenh> LichThamBenhs { get; set; }
        public decimal TongChiPhiChuaThanhToan { get; set; }
        public decimal TongChiPhiDaThanhToan { get; set; }
    }

    public class BenhNhanXuatVienViewModel
    {
        public int MaBenhNhan { get; set; }
        
        [Display(Name = "Họ tên bệnh nhân")]
        public string HoTen { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập ngày xuất viện")]
        [Display(Name = "Ngày xuất viện")]
        [DataType(DataType.Date)]
        public DateTime NgayXuatVien { get; set; } = DateTime.Now;
        
        [Display(Name = "Ghi chú")]
        public string GhiChu { get; set; }
    }

    public class BenhNhanStatisticsViewModel
    {
        public int TongSoBenhNhan { get; set; }
        public int SoBenhNhanDangDieuTri { get; set; }
        public int SoBenhNhanDaXuatVien { get; set; }
        public Dictionary<string, int> SoBenhNhanTheoKhoa { get; set; }
        public Dictionary<string, int> SoBenhNhanTheoThang { get; set; }
    }
}