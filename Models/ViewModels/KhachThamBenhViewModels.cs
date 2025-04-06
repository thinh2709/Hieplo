using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QuanLyBenhVienNoiTru.Models;

namespace QuanLyBenhVienNoiTru.Models.ViewModels
{
    public class DangKyThamViewModel
    {
        [Required(ErrorMessage = "Mã khách không được để trống")]
        [Display(Name = "Mã khách")]
        public int MaKhach { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn bệnh nhân")]
        [Display(Name = "Bệnh nhân")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn bệnh nhân")]
        public int MaBenhNhan { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn thời gian thăm")]
        [Display(Name = "Thời gian thăm")]
        [DataType(DataType.DateTime)]
        [FutureDate(ErrorMessage = "Thời gian thăm phải lớn hơn thời gian hiện tại")]
        public DateTime ThoiGianTham { get; set; }

        // This collection is only used to populate the dropdown, not for validation
        public IEnumerable<BenhNhan> BenhNhans { get; set; } = new List<BenhNhan>();
    }

    // Tạo attribute xác thực ngày phải là tương lai
    public class FutureDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value is DateTime date)
            {
                return date > DateTime.Now;
            }
            return false;
        }
    }
}