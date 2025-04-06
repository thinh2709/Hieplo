using System.Collections.Generic;
using System.Threading.Tasks;
using QuanLyBenhVienNoiTru.Models;

namespace QuanLyBenhVienNoiTru.Services
{
    public interface IBenhNhanService
    {
        Task<IEnumerable<BenhNhan>> GetAllBenhNhanAsync();
        Task<BenhNhan> GetBenhNhanByIdAsync(int id);
        Task<IEnumerable<BenhNhan>> GetBenhNhanByKhoaAsync(int khoaId);
        Task<bool> AddBenhNhanAsync(BenhNhan benhNhan);
        Task<bool> UpdateBenhNhanAsync(BenhNhan benhNhan);
        Task<bool> DeleteBenhNhanAsync(int id);
    }
}