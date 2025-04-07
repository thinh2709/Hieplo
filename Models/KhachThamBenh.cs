using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyBenhVienNoiTru.Models
{
    public class KhachThamBenh
    {
        public KhachThamBenh()
        {
            // Initialize collection to prevent null reference exceptions
            LichThamBenhs = new List<LichThamBenh>();
        }
        
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaKhach { get; set; }

        [Display(Name = "Họ tên")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
        public string HoTen { get; set; }

        [Display(Name = "Mã tài khoản")]
        public int? MaTaiKhoan { get; set; }

        [Display(Name = "Số điện thoại")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Số điện thoại phải có 10 chữ số")]
        public string SoDienThoai { get; set; }

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
        public string Email { get; set; }

        [Display(Name = "Địa chỉ")]
        [StringLength(200, ErrorMessage = "Địa chỉ không được vượt quá 200 ký tự")]
        public string DiaChi { get; set; }

        [Display(Name = "Mối quan hệ")]
        [StringLength(50, ErrorMessage = "Mối quan hệ không được vượt quá 50 ký tự")]
        public string MoiQuanHe { get; set; }

        // Navigation properties
        [ForeignKey("MaTaiKhoan")]
        public virtual TaiKhoan? TaiKhoan { get; set; }
        public virtual ICollection<LichThamBenh> LichThamBenhs { get; set; }
    }
}