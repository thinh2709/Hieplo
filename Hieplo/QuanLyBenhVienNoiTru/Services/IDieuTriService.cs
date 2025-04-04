using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QuanLyBenhVienNoiTru.Models;
using QuanLyBenhVienNoiTru.Models.ViewModels;

namespace QuanLyBenhVienNoiTru.Services
{
    public interface IDieuTriService
    {
        // Lấy danh sách tất cả điều trị
        Task<List<DieuTriBenhNhanListViewModel>> GetAllDieuTriAsync();
        
        // Lấy thông tin chi tiết một điều trị
        Task<DieuTriBenhNhanDetailsViewModel> GetDieuTriDetailsAsync(int id);
        
        // Lấy danh sách điều trị của một bệnh nhân
        Task<List<DieuTriBenhNhanListViewModel>> GetDieuTriByBenhNhanAsync(int benhNhanId);
        
        // Lấy danh sách điều trị của một bác sĩ
        Task<List<DieuTriBenhNhanListViewModel>> GetDieuTriByBacSiAsync(int bacSiId);
        
        // Tạo mới điều trị
        Task<bool> CreateDieuTriAsync(DieuTriBenhNhanCreateViewModel model);
        
        // Cập nhật điều trị
        Task<bool> UpdateDieuTriAsync(DieuTriBenhNhanEditViewModel model);
        
        // Xóa điều trị
        Task<bool> DeleteDieuTriAsync(int id);
        
        // Lấy thông tin điều trị để chỉnh sửa
        Task<DieuTriBenhNhanEditViewModel> GetDieuTriForEditAsync(int id);
        
        // Lấy tổng chi phí điều trị của một bệnh nhân
        Task<decimal> GetTotalChiPhiByBenhNhanAsync(int benhNhanId);
        
        // Cập nhật chi phí điều trị
        Task<bool> UpdateChiPhiDieuTriAsync(int benhNhanId, decimal chiPhi);
        
        // Đánh dấu đã thanh toán chi phí điều trị
        Task<bool> MarkChiPhiAsPaidAsync(int chiPhiId);
        
        // Lấy chi tiết bệnh nhân và danh sách điều trị
        Task<BenhNhanDieuTriViewModel> GetBenhNhanWithDieuTriAsync(int benhNhanId);
    }
}