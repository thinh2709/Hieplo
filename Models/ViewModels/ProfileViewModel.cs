using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLyBenhVienNoiTru.Models.ViewModels
{
    public class ProfileViewModel
    {
        public int MaTaiKhoan { get; set; }

        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; }

        [Display(Name = "Vai trò")]
        public string VaiTro { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Số điện thoại phải có 10 chữ số")]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; }

        [Display(Name = "Địa chỉ")]
        public string DiaChi { get; set; }

        // Thông tin cho bác sĩ
        [Display(Name = "Giới tính")]
        public string GioiTinh { get; set; }

        [Display(Name = "Chuyên khoa")]
        public string ChuyenKhoa { get; set; }

        [Display(Name = "Khoa")]
        public int? MaKhoa { get; set; }

        [Display(Name = "Tên khoa")]
        public string TenKhoa { get; set; }

        [Display(Name = "Ngày vào làm")]
        [DataType(DataType.Date)]
        public DateTime? NgayVaoLam { get; set; }

        // Thông tin cho khách
        [Display(Name = "Mối quan hệ")]
        public string MoiQuanHe { get; set; }

        // Thông tin đổi mật khẩu
        [Display(Name = "Mật khẩu mới")]
        [DataType(DataType.Password)]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự")]
        public string MatKhauMoi { get; set; }

        [Display(Name = "Xác nhận mật khẩu")]
        [DataType(DataType.Password)]
        [Compare("MatKhauMoi", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string XacNhanMatKhau { get; set; }
    }
} 