using System;

namespace QuanLyBenhVienNoiTru.Models.ViewModels
{
    public class HoatDongViewModel
    {
        public string Loai { get; set; }
        public DateTime ThoiGian { get; set; }
        public string NoiDung { get; set; }
        public int MaDoiTuong { get; set; }
        public string Icon { get; set; }
        public string MauSac { get; set; }
        
        public string ThoiGianHienThi
        {
            get
            {
                var thoiGianHienTai = DateTime.Now;
                var khoangCach = thoiGianHienTai - ThoiGian;
                
                if (khoangCach.TotalMinutes < 1)
                    return "Vừa xong";
                if (khoangCach.TotalMinutes < 60)
                    return $"{(int)khoangCach.TotalMinutes} phút trước";
                if (khoangCach.TotalHours < 24)
                    return $"{(int)khoangCach.TotalHours} giờ trước";
                if (khoangCach.TotalDays < 7)
                    return $"{(int)khoangCach.TotalDays} ngày trước";
                
                return ThoiGian.ToString("dd/MM/yyyy HH:mm");
            }
        }
        
        public string UrlChiTiet
        {
            get
            {
                return Loai switch
                {
                    "BenhNhanMoi" => $"/BenhNhan/Details/{MaDoiTuong}",
                    "DieuTriMoi" => $"/DieuTriBenhNhan/Details/{MaDoiTuong}",
                    "ChiPhiMoi" => $"/ChiPhiDieuTri/Details/{MaDoiTuong}",
                    _ => "#"
                };
            }
        }
    }
} 