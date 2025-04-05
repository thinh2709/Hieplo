namespace QuanLyBenhVienNoiTru.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class BenhNhan
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaBenhNhan { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; }

        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        [CustomValidation(typeof(BenhNhan), nameof(ValidateNgaySinh))]
        public DateTime? NgaySinh { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giới tính")]
        [Display(Name = "Giới tính")]
        public string GioiTinh { get; set; }

        [Display(Name = "Số điện thoại")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string SoDienThoai { get; set; }

        [Display(Name = "Địa chỉ")]
        public string DiaChi { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Display(Name = "Có bảo hiểm y tế")]
        public bool BaoHiemYTe { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập ngày nhập viện")]
        [Display(Name = "Ngày nhập viện")]
        [DataType(DataType.Date)]
        public DateTime NgayNhapVien { get; set; }

        [Display(Name = "Ngày xuất viện")]
        [DataType(DataType.Date)]
        public DateTime? NgayXuatVien { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn khoa")]
        [Display(Name = "Khoa")]
        public int MaKhoa { get; set; }

        [Display(Name = "Bác sĩ phụ trách")]
        public int? MaBacSi { get; set; }

        [Display(Name = "Chẩn đoán")]
        public string ChanDoan { get; set; }

        public bool TrangThai { get; set; }

        // Navigation properties
        public virtual Khoa Khoa { get; set; }
        public virtual BacSi BacSi { get; set; }
        public virtual ICollection<DieuTriBenhNhan> DieuTriBenhNhans { get; set; }
        public virtual ICollection<LichThamBenh> LichThamBenhs { get; set; }
        public virtual ICollection<ChiPhiDieuTri> ChiPhiDieuTris { get; set; }
        public virtual ICollection<HinhThucDieuTri> HinhThucDieuTris { get; set; }

        public static ValidationResult ValidateNgaySinh(DateTime? ngaySinh, ValidationContext context)
        {
            if (ngaySinh.HasValue && ngaySinh.Value > DateTime.Now)
            {
                return new ValidationResult("Ngày sinh phải nhỏ hơn ngày hiện tại");
            }
            return ValidationResult.Success;
        }
    }
}