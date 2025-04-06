using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyBenhVienNoiTru.Models
{
    public class Giuong
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaGiuong { get; set; }
        
        [Required(ErrorMessage = "Tên giường không được để trống")]
        [Display(Name = "Tên giường")]
        [StringLength(50, ErrorMessage = "Tên giường không được vượt quá 50 ký tự")]
        public string TenGiuong { get; set; }
        
        [Required(ErrorMessage = "Giá theo ngày không được để trống")]
        [Display(Name = "Giá theo ngày")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá không được âm")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal GiaTheoNgay { get; set; }
        
        [Required(ErrorMessage = "Trạng thái không được để trống")]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } // "Trống", "Đã sử dụng", "Đang sửa chữa"
        
        [Required(ErrorMessage = "Khoa không được để trống")]
        [Display(Name = "Mã khoa")]
        public int MaKhoa { get; set; }
        
        [Display(Name = "Mã bệnh nhân")]
        public int? MaBenhNhan { get; set; }
        
        [Display(Name = "Mô tả")]
        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string MoTa { get; set; }
        
        // Navigation properties
        [ForeignKey("MaKhoa")]
        public virtual Khoa Khoa { get; set; }
        
        [ForeignKey("MaBenhNhan")]
        public virtual BenhNhan BenhNhan { get; set; }
    }
} 