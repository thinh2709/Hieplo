using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBenhVienNoiTru.Data;
using QuanLyBenhVienNoiTru.Models;
using QuanLyBenhVienNoiTru.Filters;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyBenhVienNoiTru.Controllers
{
    [AuthorizeRoles("Admin")]
    public class HinhThucDieuTriController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HinhThucDieuTriController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var hinhThucDieuTris = await _context.HinhThucDieuTris
                .Include(h => h.Khoa)
                .ToListAsync();
            return View(hinhThucDieuTris);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hinhThucDieuTri = await _context.HinhThucDieuTris
                .Include(h => h.Khoa)
                .FirstOrDefaultAsync(m => m.MaDieuTri == id);
                
            if (hinhThucDieuTri == null)
            {
                return NotFound();
            }

            return View(hinhThucDieuTri);
        }

        public IActionResult Create()
        {
            ViewBag.MaKhoa = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Khoas, "MaKhoa", "TenKhoa");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HinhThucDieuTri hinhThucDieuTri)
        {
            if (ModelState.IsValid)
            {
                _context.Add(hinhThucDieuTri);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.MaKhoa = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Khoas, "MaKhoa", "TenKhoa", hinhThucDieuTri.MaKhoa);
            return View(hinhThucDieuTri);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hinhThucDieuTri = await _context.HinhThucDieuTris.FindAsync(id);
            if (hinhThucDieuTri == null)
            {
                return NotFound();
            }
            ViewBag.MaKhoa = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Khoas, "MaKhoa", "TenKhoa", hinhThucDieuTri.MaKhoa);
            return View(hinhThucDieuTri);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, HinhThucDieuTri hinhThucDieuTri)
        {
            if (id != hinhThucDieuTri.MaDieuTri)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(hinhThucDieuTri);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.HinhThucDieuTris.AnyAsync(e => e.MaDieuTri == id))
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
            ViewBag.MaKhoa = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Khoas, "MaKhoa", "TenKhoa", hinhThucDieuTri.MaKhoa);
            return View(hinhThucDieuTri);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hinhThucDieuTri = await _context.HinhThucDieuTris
                .FirstOrDefaultAsync(m => m.MaDieuTri == id);
                
            if (hinhThucDieuTri == null)
            {
                return NotFound();
            }

            return View(hinhThucDieuTri);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hinhThucDieuTri = await _context.HinhThucDieuTris.FindAsync(id);
            _context.HinhThucDieuTris.Remove(hinhThucDieuTri);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}