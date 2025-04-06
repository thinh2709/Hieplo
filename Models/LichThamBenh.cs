using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyBenhVienNoiTru.Models
{
    public class LichThamBenh
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaLich { get; set; }
        
        public int? MaKhach { get; set; }
        public int? MaBenhNhan { get; set; }
        public DateTime? ThoiGianTham { get; set; }
        public string TrangThai { get; set; }

        // Navigation properties
        [ForeignKey("MaKhach")]
        public virtual KhachThamBenh KhachThamBenh { get; set; }
        
        [ForeignKey("MaBenhNhan")]
        public virtual BenhNhan BenhNhan { get; set; }
    }
}