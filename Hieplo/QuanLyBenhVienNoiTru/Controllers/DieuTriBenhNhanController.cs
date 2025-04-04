using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyBenhVienNoiTru.Data;
using QuanLyBenhVienNoiTru.Models;
using QuanLyBenhVienNoiTru.Filters;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyBenhVienNoiTru.Controllers
{
    [AuthorizeRoles("Admin", "Bác sĩ")]
    public class DieuTriBenhNhanController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DieuTriBenhNhanController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var dieuTriList = await _context.DieuTriBenhNhans
                .Include(d => d.BenhNhan)
                .Include(d => d.HinhThucDieuTri)
                .Include(d => d.BacSi)
                .ToListAsync();
                
            return View(dieuTriList);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dieuTri = await _context.DieuTriBenhNhans
                .Include(d => d.BenhNhan)
                .Include(d => d.HinhThucDieuTri)
                .Include(d => d.BacSi)
                .FirstOrDefaultAsync(d => d.MaDieuTriBenhNhan == id);
                
            if (dieuTri == null)
            {
                return NotFound();
            }

            return View(dieuTri);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.MaBenhNhan = new SelectList(await _context.BenhNhans.ToListAsync(), "MaBenhNhan", "HoTen");
            ViewBag.MaDieuTri = new SelectList(await _context.HinhThucDieuTris.ToListAsync(), "MaDieuTri", "TenDieuTri");
            ViewBag.MaBacSi = new SelectList(await _context.BacSis.ToListAsync(), "MaBacSi", "HoTen");
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DieuTriBenhNhan dieuTriBenhNhan)
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
            
            ViewBag.MaBenhNhan = new SelectList(await _context.BenhNhans.ToListAsync(), "MaBenhNhan", "HoTen", dieuTriBenhNhan.MaBenhNhan);
            ViewBag.MaDieuTri = new SelectList(await _context.HinhThucDieuTris.ToListAsync(), "MaDieuTri", "TenDieuTri", dieuTriBenhNhan.MaDieuTri);
            ViewBag.MaBacSi = new SelectList(await _context.BacSis.ToListAsync(), "MaBacSi", "HoTen", dieuTriBenhNhan.MaBacSi);
            
            return View(dieuTriBenhNhan);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dieuTri = await _context.DieuTriBenhNhans.FindAsync(id);
            if (dieuTri == null)
            {
                return NotFound();
            }
            
            ViewBag.MaBenhNhan = new SelectList(await _context.BenhNhans.ToListAsync(), "MaBenhNhan", "HoTen", dieuTri.MaBenhNhan);
            ViewBag.MaDieuTri = new SelectList(await _context.HinhThucDieuTris.ToListAsync(), "MaDieuTri", "TenDieuTri", dieuTri.MaDieuTri);
            ViewBag.MaBacSi = new SelectList(await _context.BacSis.ToListAsync(), "MaBacSi", "HoTen", dieuTri.MaBacSi);
            
            return View(dieuTri);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DieuTriBenhNhan dieuTriBenhNhan)
        {
            if (id != dieuTriBenhNhan.MaDieuTriBenhNhan)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dieuTriBenhNhan);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.DieuTriBenhNhans.AnyAsync(e => e.MaDieuTriBenhNhan == id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.MaBenhNhan = new SelectList(await _context.BenhNhans.ToListAsync(), "MaBenhNhan", "HoTen", dieuTriBenhNhan.MaBenhNhan);
            ViewBag.MaDieuTri = new SelectList(await _context.HinhThucDieuTris.ToListAsync(), "MaDieuTri", "TenDieuTri", dieuTriBenhNhan.MaDieuTri);
            ViewBag.MaBacSi = new SelectList(await _context.BacSis.ToListAsync(), "MaBacSi", "HoTen", dieuTriBenhNhan.MaBacSi);
            
            return View(dieuTriBenhNhan);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dieuTri = await _context.DieuTriBenhNhans
                .Include(d => d.BenhNhan)
                .Include(d => d.HinhThucDieuTri)
                .Include(d => d.BacSi)
                .FirstOrDefaultAsync(d => d.MaDieuTriBenhNhan == id);
                
            if (dieuTri == null)
            {
                return NotFound();
            }

            return View(dieuTri);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dieuTri = await _context.DieuTriBenhNhans.FindAsync(id);
            _context.DieuTriBenhNhans.Remove(dieuTri);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}