using Microsoft.EntityFrameworkCore;
using QuanLyBenhVienNoiTru.Data;
using QuanLyBenhVienNoiTru.Models;
using QuanLyBenhVienNoiTru.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyBenhVienNoiTru.Services
{
    public class DieuTriService : IDieuTriService
    {
        private readonly ApplicationDbContext _context;

        public DieuTriService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<DieuTriBenhNhanListViewModel>> GetAllDieuTriAsync()
        {
            return await _context.DieuTriBenhNhans
                .Include(d => d.BenhNhan)
                .Include(d => d.HinhThucDieuTri)
                .Include(d => d.BacSi)
                .Select(d => new DieuTriBenhNhanListViewModel
                {
                    MaDieuTriBenhNhan = d.MaDieuTriBenhNhan,
                    TenBenhNhan = d.BenhNhan.HoTen,
                    TenDieuTri = d.HinhThucDieuTri.TenDieuTri,
                    TenBacSi = d.BacSi.HoTen,
                    NgayThucHien = d.NgayThucHien,
                    KetQua = d.KetQua,
                    ChiPhi = d.HinhThucDieuTri.ChiPhi
                })
                .ToListAsync();
        }

        public async Task<DieuTriBenhNhanDetailsViewModel> GetDieuTriDetailsAsync(int id)
        {
            return await _context.DieuTriBenhNhans
                .Include(d => d.BenhNhan)
                .Include(d => d.HinhThucDieuTri)
                .Include(d => d.BacSi)
                .Where(d => d.MaDieuTriBenhNhan == id)
                .Select(d => new DieuTriBenhNhanDetailsViewModel
                {
                    MaDieuTriBenhNhan = d.MaDieuTriBenhNhan,
                    TenBenhNhan = d.BenhNhan.HoTen,
                    TenDieuTri = d.HinhThucDieuTri.TenDieuTri,
                    TenBacSi = d.BacSi.HoTen,
                    NgayThucHien = d.NgayThucHien,
                    KetQua = d.KetQua,
                    ChiPhi = d.HinhThucDieuTri.ChiPhi,
                    DaThanhToan = _context.ChiPhiDieuTris
                        .Any(c => c.MaBenhNhan == d.MaBenhNhan && c.DaThanhToan)
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<DieuTriBenhNhanListViewModel>> GetDieuTriByBenhNhanAsync(int benhNhanId)
        {
            return await _context.DieuTriBenhNhans
                .Include(d => d.HinhThucDieuTri)
                .Include(d => d.BacSi)
                .Where(d => d.MaBenhNhan == benhNhanId)
                .Select(d => new DieuTriBenhNhanListViewModel
                {
                    MaDieuTriBenhNhan = d.MaDieuTriBenhNhan,
                    TenBenhNhan = d.BenhNhan.HoTen,
                    TenDieuTri = d.HinhThucDieuTri.TenDieuTri,
                    TenBacSi = d.BacSi.HoTen,
                    NgayThucHien = d.NgayThucHien,
                    KetQua = d.KetQua,
                    ChiPhi = d.HinhThucDieuTri.ChiPhi
                })
                .ToListAsync();
        }

        public async Task<List<DieuTriBenhNhanListViewModel>> GetDieuTriByBacSiAsync(int bacSiId)
        {
            return await _context.DieuTriBenhNhans
                .Include(d => d.BenhNhan)
                .Include(d => d.HinhThucDieuTri)
                .Where(d => d.MaBacSi == bacSiId)
                .Select(d => new DieuTriBenhNhanListViewModel
                {
                    MaDieuTriBenhNhan = d.MaDieuTriBenhNhan,
                    TenBenhNhan = d.BenhNhan.HoTen,
                    TenDieuTri = d.HinhThucDieuTri.TenDieuTri,
                    TenBacSi = d.BacSi.HoTen,
                    NgayThucHien = d.NgayThucHien,
                    KetQua = d.KetQua,
                    ChiPhi = d.HinhThucDieuTri.ChiPhi
                })
                .ToListAsync();
        }

        public async Task<bool> CreateDieuTriAsync(DieuTriBenhNhanCreateViewModel model)
        {
            try
            {
                var dieuTri = new DieuTriBenhNhan
                {
                    MaBenhNhan = model.MaBenhNhan,
                    MaDieuTri = model.MaDieuTri,
                    MaBacSi = model.MaBacSi,
                    NgayThucHien = model.NgayThucHien,
                    KetQua = model.KetQua
                };

                _context.DieuTriBenhNhans.Add(dieuTri);
                await _context.SaveChangesAsync();

                // Cập nhật chi phí điều trị
                var hinhThucDieuTri = await _context.HinhThucDieuTris.FindAsync(model.MaDieuTri);
                var chiPhiHienTai = await _context.ChiPhiDieuTris
                    .FirstOrDefaultAsync(c => c.MaBenhNhan == model.MaBenhNhan && !c.DaThanhToan);

                if (chiPhiHienTai == null)
                {
                    chiPhiHienTai = new ChiPhiDieuTri
                    {
                        MaBenhNhan = model.MaBenhNhan.Value,
                        TongChiPhi = hinhThucDieuTri.ChiPhi,
                        DaThanhToan = false,
                        NgayLap = DateTime.Now
                    };
                    _context.ChiPhiDieuTris.Add(chiPhiHienTai);
                }
                else
                {
                    chiPhiHienTai.TongChiPhi += hinhThucDieuTri.ChiPhi;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateDieuTriAsync(DieuTriBenhNhanEditViewModel model)
        {
            try
            {
                var dieuTri = await _context.DieuTriBenhNhans.FindAsync(model.MaDieuTriBenhNhan);
                if (dieuTri == null)
                {
                    return false;
                }

                // Lưu lại mã điều trị cũ để xử lý chi phí
                var oldMaDieuTri = dieuTri.MaDieuTri;

                dieuTri.MaBenhNhan = model.MaBenhNhan;
                dieuTri.MaDieuTri = model.MaDieuTri;
                dieuTri.MaBacSi = model.MaBacSi;
                dieuTri.NgayThucHien = model.NgayThucHien;
                dieuTri.KetQua = model.KetQua;

                _context.Update(dieuTri);
                await _context.SaveChangesAsync();

                // Cập nhật chi phí nếu thay đổi hình thức điều trị
                if (oldMaDieuTri != model.MaDieuTri)
                {
                    var oldHinhThuc = await _context.HinhThucDieuTris.FindAsync(oldMaDieuTri);
                    var newHinhThuc = await _context.HinhThucDieuTris.FindAsync(model.MaDieuTri);
                    
                    var chiPhiHienTai = await _context.ChiPhiDieuTris
                        .FirstOrDefaultAsync(c => c.MaBenhNhan == model.MaBenhNhan && !c.DaThanhToan);

                    if (chiPhiHienTai != null)
                    {
                        // Trừ chi phí cũ và cộng chi phí mới
                        chiPhiHienTai.TongChiPhi = chiPhiHienTai.TongChiPhi - oldHinhThuc.ChiPhi + newHinhThuc.ChiPhi;
                        _context.Update(chiPhiHienTai);
                        await _context.SaveChangesAsync();
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteDieuTriAsync(int id)
        {
            try
            {
                var dieuTri = await _context.DieuTriBenhNhans
                    .Include(d => d.HinhThucDieuTri)
                    .FirstOrDefaultAsync(d => d.MaDieuTriBenhNhan == id);
                    
                if (dieuTri == null)
                {
                    return false;
                }

                // Cập nhật chi phí
                var chiPhiHienTai = await _context.ChiPhiDieuTris
                    .FirstOrDefaultAsync(c => c.MaBenhNhan == dieuTri.MaBenhNhan && !c.DaThanhToan);

                if (chiPhiHienTai != null)
                {
                    chiPhiHienTai.TongChiPhi -= dieuTri.HinhThucDieuTri.ChiPhi;
                    _context.Update(chiPhiHienTai);
                }

                _context.DieuTriBenhNhans.Remove(dieuTri);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<DieuTriBenhNhanEditViewModel> GetDieuTriForEditAsync(int id)
        {
            return await _context.DieuTriBenhNhans
                .Where(d => d.MaDieuTriBenhNhan == id)
                .Select(d => new DieuTriBenhNhanEditViewModel
                {
                    MaDieuTriBenhNhan = d.MaDieuTriBenhNhan,
                    MaBenhNhan = d.MaBenhNhan,
                    MaDieuTri = d.MaDieuTri,
                    MaBacSi = d.MaBacSi,
                    NgayThucHien = d.NgayThucHien,
                    KetQua = d.KetQua
                })
                .FirstOrDefaultAsync();
        }

        public async Task<decimal> GetTotalChiPhiByBenhNhanAsync(int benhNhanId)
        {
            var chiPhi = await _context.ChiPhiDieuTris
                .Where(c => c.MaBenhNhan == benhNhanId && !c.DaThanhToan)
                .FirstOrDefaultAsync();

            return chiPhi?.TongChiPhi ?? 0;
        }

        public async Task<bool> UpdateChiPhiDieuTriAsync(int benhNhanId, decimal chiPhi)
        {
            try
            {
                var chiPhiHienTai = await _context.ChiPhiDieuTris
                    .FirstOrDefaultAsync(c => c.MaBenhNhan == benhNhanId && !c.DaThanhToan);

                if (chiPhiHienTai == null)
                {
                    chiPhiHienTai = new ChiPhiDieuTri
                    {
                        MaBenhNhan = benhNhanId,
                        TongChiPhi = chiPhi,
                        DaThanhToan = false,
                        NgayLap = DateTime.Now
                    };
                    _context.Add(chiPhiHienTai);
                }
                else
                {
                    chiPhiHienTai.TongChiPhi = chiPhi;
                    _context.Update(chiPhiHienTai);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> MarkChiPhiAsPaidAsync(int chiPhiId)
        {
            try
            {
                var chiPhi = await _context.ChiPhiDieuTris.FindAsync(chiPhiId);
                if (chiPhi == null)
                {
                    return false;
                }

                chiPhi.DaThanhToan = true;
                _context.Update(chiPhi);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<BenhNhanDieuTriViewModel> GetBenhNhanWithDieuTriAsync(int benhNhanId)
        {
            var benhNhan = await _context.BenhNhans
                .Include(b => b.Khoa)
                .FirstOrDefaultAsync(b => b.MaBenhNhan == benhNhanId);

            if (benhNhan == null)
            {
                return null;
            }

            var danhSachDieuTri = await GetDieuTriByBenhNhanAsync(benhNhanId);
            var chiPhi = await _context.ChiPhiDieuTris
                .Where(c => c.MaBenhNhan == benhNhanId)
                .OrderByDescending(c => c.NgayLap)
                .FirstOrDefaultAsync();

            return new BenhNhanDieuTriViewModel
            {
                MaBenhNhan = benhNhan.MaBenhNhan,
                HoTen = benhNhan.HoTen,
                TenKhoa = benhNhan.Khoa?.TenKhoa,
                NgayNhapVien = benhNhan.NgayNhapVien,
                TongChiPhi = chiPhi?.TongChiPhi ?? 0,
                DaThanhToan = chiPhi?.DaThanhToan ?? false,
                DanhSachDieuTri = danhSachDieuTri
            };
        }
    }
}