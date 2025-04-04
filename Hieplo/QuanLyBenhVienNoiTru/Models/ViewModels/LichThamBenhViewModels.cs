using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QuanLyBenhVienNoiTru.Models;

namespace QuanLyBenhVienNoiTru.Models.ViewModels
{
    public class LichThamBenhViewModel
    {
        public int MaLich { get; set; }
        
        [Display(Name = "Khách thăm")]
        public int MaKhach { get; set; }
        
        [Display(Name = "Bệnh nhân")]
        public int MaBenhNhan { get; set; }
        
        [Display(Name = "Thời gian thăm")]
        public DateTime ThoiGianTham { get; set; }
        
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; }
        
        public KhachThamBenh KhachThamBenh { get; set; }
        public BenhNhan BenhNhan { get; set; }
    }
}