using Microsoft.EntityFrameworkCore;
using QuanLyBenhVienNoiTru.Data;
using QuanLyBenhVienNoiTru.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyBenhVienNoiTru.Services
{
    public class BenhNhanService : IBenhNhanService
    {
        private readonly ApplicationDbContext _context;

        public BenhNhanService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BenhNhan>> GetAllBenhNhanAsync()
        {
            return await _context.BenhNhans
                .Include(b => b.Khoa)
                .ToListAsync();
        }

        public async Task<BenhNhan> GetBenhNhanByIdAsync(int id)
        {
            return await _context.BenhNhans
                .Include(b => b.Khoa)
                .Include(b => b.DieuTriBenhNhans)
                .ThenInclude(d => d.BacSi)
                .Include(b => b.DieuTriBenhNhans)
                .ThenInclude(d => d.HinhThucDieuTri)
                .Include(b => b.ChiPhiDieuTris)
                .FirstOrDefaultAsync(b => b.MaBenhNhan == id);
        }

        public async Task<IEnumerable<BenhNhan>> GetBenhNhanByKhoaAsync(int khoaId)
        {
            return await _context.BenhNhans
                .Where(b => b.MaKhoa == khoaId)
                .ToListAsync();
        }

        public async Task<bool> AddBenhNhanAsync(BenhNhan benhNhan)
        {
            try
            {
                _context.BenhNhans.Add(benhNhan);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateBenhNhanAsync(BenhNhan benhNhan)
        {
            try
            {
                _context.BenhNhans.Update(benhNhan);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteBenhNhanAsync(int id)
        {
            try
            {
                var benhNhan = await _context.BenhNhans.FindAsync(id);
                if (benhNhan == null)
                    return false;

                _context.BenhNhans.Remove(benhNhan);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}