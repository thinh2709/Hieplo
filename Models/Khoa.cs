using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyBenhVienNoiTru.Models
{
    public class Khoa
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaKhoa { get; set; }

        [Required(ErrorMessage = "Tên khoa không được để trống")]
        [Display(Name = "Tên khoa")]
        [StringLength(100, ErrorMessage = "Tên khoa không được vượt quá 100 ký tự")]
        public string TenKhoa { get; set; }

        [Display(Name = "Mô tả")]
        public string MoTa { get; set; }

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; }

        // Navigation properties
        public virtual ICollection<BenhNhan> BenhNhans { get; set; } = new List<BenhNhan>();
        public virtual ICollection<BacSi> BacSis { get; set; } = new List<BacSi>();
        public virtual ICollection<HinhThucDieuTri> HinhThucDieuTris { get; set; } = new List<HinhThucDieuTri>();
        public virtual ICollection<Giuong> Giuongs { get; set; } = new List<Giuong>();
    }
}