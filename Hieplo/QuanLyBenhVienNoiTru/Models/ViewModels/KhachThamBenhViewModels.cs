using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QuanLyBenhVienNoiTru.Models;

namespace QuanLyBenhVienNoiTru.Models.ViewModels
{
    public class DangKyThamViewModel
    {
        public int MaKhach { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn bệnh nhân")]
        [Display(Name = "Bệnh nhân")]
        public int MaBenhNhan { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn thời gian thăm")]
        [Display(Name = "Thời gian thăm")]
        public DateTime ThoiGianTham { get; set; }

        public IEnumerable<BenhNhan> BenhNhans { get; set; }
    }
}