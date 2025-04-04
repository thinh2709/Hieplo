using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBenhVienNoiTru.Data;
using QuanLyBenhVienNoiTru.Models;
using QuanLyBenhVienNoiTru.Filters;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace QuanLyBenhVienNoiTru.Controllers
{
    [AuthorizeRoles("Admin", "Bác sĩ")]
    public class BacSiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BacSiController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue("UserId");
            var bacSiId = 0;
            
            if (User.IsInRole("Bác sĩ") && int.TryParse(userId, out int maTaiKhoan))
            {
                var bacSi = await _context.BacSis.FirstOrDefaultAsync(b => b.MaTaiKhoan == maTaiKhoan);
                if (bacSi != null)
                {
                    bacSiId = bacSi.MaBacSi;
                }
            }
            
            var benhNhanList = await _context.BenhNhans
                .Where(b => b.NgayXuatVien == null)
                .Include(b => b.Khoa)
                .Include(b => b.DieuTriBenhNhans)
                .ThenInclude(d => d.BacSi)
                .ToListAsync();
                
            if (User.IsInRole("Bác sĩ"))
            {
                benhNhanList = benhNhanList
                    .Where(b => b.DieuTriBenhNhans.Any(d => d.MaBacSi == bacSiId) || !b.DieuTriBenhNhans.Any())
                    .ToList();
            }
            
            return View(benhNhanList);
        }
        
        public async Task<IActionResult> ThemDieuTri(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var benhNhan = await _context.BenhNhans
                .Include(b => b.Khoa)
                .FirstOrDefaultAsync(b => b.MaBenhNhan == id);
                
            if (benhNhan == null)
            {
                return NotFound();
            }
            
            ViewBag.MaDieuTri = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _context.HinhThucDieuTris.ToListAsync(), "MaDieuTri", "TenDieuTri");
                
            ViewBag.MaBacSi = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _context.BacSis.ToListAsync(), "MaBacSi", "HoTen");
                
            var dieuTri = new DieuTriBenhNhan
            {
                MaBenhNhan = benhNhan.MaBenhNhan,
                NgayThucHien = DateTime.Now
            };
            
            return View(dieuTri);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemDieuTri(DieuTriBenhNhan dieuTriBenhNhan)
        {
            if (ModelState.IsValid)
            {
                _context.Add(dieuTriBenhNhan);
                await _context.SaveChangesAsync();
                
                // Cập nhật chi phí
                var hinhThucDieuTri = await _context.HinhThucDieuTris
                    .FindAsync(dieuTriBenhNhan.MaDieuTri);
                    
                var chiPhiHienTai = await _context.ChiPhiDieuTris
                    .FirstOrDefaultAsync(c => c.MaBenhNhan == dieuTriBenhNhan.MaBenhNhan && !c.DaThanhToan);
                    
                if (chiPhiHienTai == null)
                {
                    chiPhiHienTai = new ChiPhiDieuTri
                    {
                        MaBenhNhan = dieuTriBenhNhan.MaBenhNhan.Value,
                        TongChiPhi = hinhThucDieuTri.ChiPhi,
                        DaThanhToan = false,
                        NgayLap = DateTime.Now
                    };
                    _context.Add(chiPhiHienTai);
                }
                else
                {
                    chiPhiHienTai.TongChiPhi += hinhThucDieuTri.ChiPhi;
                }
                
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.MaDieuTri = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _context.HinhThucDieuTris.ToListAsync(), "MaDieuTri", "TenDieuTri");
                
            ViewBag.MaBacSi = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _context.BacSis.ToListAsync(), "MaBacSi", "HoTen");
                
            return View(dieuTriBenhNhan);
        }
        
        public async Task<IActionResult> XuatVien(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var benhNhan = await _context.BenhNhans
                .Include(b => b.ChiPhiDieuTris)
                .FirstOrDefaultAsync(b => b.MaBenhNhan == id);
                
            if (benhNhan == null)
            {
                return NotFound();
            }
            
            if (benhNhan.NgayXuatVien != null)
            {
                return RedirectToAction(nameof(Index));
            }
            
            var chiPhi = benhNhan.ChiPhiDieuTris.FirstOrDefault(c => !c.DaThanhToan);
            
            if (chiPhi != null && !chiPhi.DaThanhToan)
            {
                TempData["Error"] = "Bệnh nhân còn chi phí chưa thanh toán. Vui lòng thanh toán trước khi xuất viện.";
                return RedirectToAction("Details", "BenhNhan", new { id = benhNhan.MaBenhNhan });
            }
            
            return View(benhNhan);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XuatVienConfirmed(int id)
        {
            var benhNhan = await _context.BenhNhans.FindAsync(id);
            
            if (benhNhan == null)
            {
                return NotFound();
            }
            
            benhNhan.NgayXuatVien = DateTime.Now;
            _context.Update(benhNhan);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }
    }
}