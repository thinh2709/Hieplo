using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyBenhVienNoiTru.Models
{
    public class HinhThucDieuTri
    {
        public HinhThucDieuTri()
        {
            // Initialize collection to prevent null reference exceptions
            DieuTriBenhNhans = new HashSet<DieuTriBenhNhan>();
        }
        
        [Key]
        [Display(Name = "Mã điều trị")]
        public int MaDieuTri { get; set; }

        [Required(ErrorMessage = "Tên điều trị không được để trống")]
        [StringLength(100, ErrorMessage = "Tên điều trị không được vượt quá 100 ký tự")]
        [Display(Name = "Tên điều trị")]
        public string TenDieuTri { get; set; }

        [Required(ErrorMessage = "Khoa áp dụng không được để trống")]
        [Display(Name = "Khoa áp dụng")]
        public int MaKhoa { get; set; }

        [Required(ErrorMessage = "Chi phí không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Chi phí không được âm")]
        [Display(Name = "Chi phí")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal ChiPhi { get; set; }

        [Required(ErrorMessage = "Mô tả không được để trống")]
        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        [Display(Name = "Mô tả")]
        public string MoTa { get; set; }

        // Navigation properties
        [ForeignKey("MaKhoa")]
        public virtual Khoa? Khoa { get; set; }
        
        public virtual ICollection<DieuTriBenhNhan> DieuTriBenhNhans { get; set; }
    }
}
