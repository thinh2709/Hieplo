using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyBenhVienNoiTru.Models
{
    public class BacSi
    {
        public BacSi()
        {
            // Initialize collections to prevent null reference exceptions
            DieuTriBenhNhans = new HashSet<DieuTriBenhNhan>();
            BenhNhans = new HashSet<BenhNhan>();
            HinhThucDieuTris = new HashSet<HinhThucDieuTri>();
        }
        
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaBacSi { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [Display(Name = "Họ tên")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
        public string HoTen { get; set; }

        [Display(Name = "Mã tài khoản")]
        public int? MaTaiKhoan { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập chuyên khoa")]
        [Display(Name = "Chuyên khoa")]
        [StringLength(100, ErrorMessage = "Chuyên khoa không được vượt quá 100 ký tự")]
        public string ChuyenKhoa { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Display(Name = "Số điện thoại")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Số điện thoại phải có 10 chữ số")]
        public string SoDienThoai { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giới tính")]
        [Display(Name = "Giới tính")]
        public string GioiTinh { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        [Display(Name = "Địa chỉ")]
        [StringLength(200, ErrorMessage = "Địa chỉ không được vượt quá 200 ký tự")]
        public string DiaChi { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập ngày vào làm")]
        [Display(Name = "Ngày vào làm")]
        [DataType(DataType.Date)]
        public DateTime NgayVaoLam { get; set; } = DateTime.Now;

        [Display(Name = "Ngày nghỉ việc")]
        [DataType(DataType.Date)]
        public DateTime? NgayNghiViec { get; set; }

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;

        [Required(ErrorMessage = "Vui lòng chọn khoa")]
        [Display(Name = "Khoa")]
        public int MaKhoa { get; set; }

        // Navigation properties
        [ForeignKey("MaTaiKhoan")]
        public virtual TaiKhoan? TaiKhoan { get; set; }

        [ForeignKey("MaKhoa")]
        public virtual Khoa? Khoa { get; set; }

        public virtual ICollection<DieuTriBenhNhan> DieuTriBenhNhans { get; set; }
        public virtual ICollection<BenhNhan> BenhNhans { get; set; }
        public virtual ICollection<HinhThucDieuTri> HinhThucDieuTris { get; set; }
    }
}