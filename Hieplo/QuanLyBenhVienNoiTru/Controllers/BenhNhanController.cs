using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyBenhVienNoiTru.Data;
using QuanLyBenhVienNoiTru.Filters;
using QuanLyBenhVienNoiTru.Models;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyBenhVienNoiTru.Controllers
{
    [AuthorizeRoles("Admin", "Bác sĩ")]
    public class BenhNhanController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BenhNhanController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var benhNhans = await _context.BenhNhans
                .Include(b => b.Khoa)
                .ToListAsync();
                
            return View(benhNhans);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var benhNhan = await _context.BenhNhans
                .Include(b => b.Khoa)
                .Include(b => b.DieuTriBenhNhans)
                    .ThenInclude(d => d.HinhThucDieuTri)
                .Include(b => b.DieuTriBenhNhans)
                    .ThenInclude(d => d.BacSi)
                .Include(b => b.ChiPhiDieuTris)
                .FirstOrDefaultAsync(m => m.MaBenhNhan == id);
                
            if (benhNhan == null)
            {
                return NotFound();
            }

            return View(benhNhan);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.MaKhoa = new SelectList(await _context.Khoas.ToListAsync(), "MaKhoa", "TenKhoa");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BenhNhan benhNhan)
        {
            if (ModelState.IsValid)
            {
                _context.Add(benhNhan);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.MaKhoa = new SelectList(await _context.Khoas.ToListAsync(), "MaKhoa", "TenKhoa", benhNhan.MaKhoa);
            return View(benhNhan);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var benhNhan = await _context.BenhNhans.FindAsync(id);
            if (benhNhan == null)
            {
                return NotFound();
            }

            // Truyền danh sách giới tính vào ViewBag
            ViewBag.GioiTinhList = new List<SelectListItem>
            {
                new SelectListItem { Value = "Nam", Text = "Nam" },
                new SelectListItem { Value = "Nữ", Text = "Nữ" },
                new SelectListItem { Value = "Khác", Text = "Khác" }
            };

            ViewBag.MaKhoa = new SelectList(await _context.Khoas.ToListAsync(), "MaKhoa", "TenKhoa", benhNhan.MaKhoa);
            return View(benhNhan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BenhNhan benhNhan)
        {
            if (id != benhNhan.MaBenhNhan)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(benhNhan);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.BenhNhans.AnyAsync(e => e.MaBenhNhan == id))
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
            
            ViewBag.MaKhoa = new SelectList(await _context.Khoas.ToListAsync(), "MaKhoa", "TenKhoa", benhNhan.MaKhoa);
            return View(benhNhan);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var benhNhan = await _context.BenhNhans
                .Include(b => b.Khoa)
                .FirstOrDefaultAsync(m => m.MaBenhNhan == id);
                
            if (benhNhan == null)
            {
                return NotFound();
            }

            return View(benhNhan);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var benhNhan = await _context.BenhNhans.FindAsync(id);
            _context.BenhNhans.Remove(benhNhan);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}