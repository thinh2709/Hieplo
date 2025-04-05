using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBenhVienNoiTru.Data;
using QuanLyBenhVienNoiTru.Models;
using QuanLyBenhVienNoiTru.Filters;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyBenhVienNoiTru.Controllers
{
    [AuthorizeRoles("Admin", "Bác sĩ")]
    public class ChiPhiDieuTriController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChiPhiDieuTriController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var chiPhiList = await _context.ChiPhiDieuTris
                .Include(c => c.BenhNhan)
                .ToListAsync();
                
            return View(chiPhiList);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chiPhi = await _context.ChiPhiDieuTris
                .Include(c => c.BenhNhan)
                    .ThenInclude(b => b.Khoa)
                .FirstOrDefaultAsync(c => c.MaChiPhi == id);
                
            if (chiPhi == null)
            {
                return NotFound();
            }

            // Lấy danh sách các điều trị của bệnh nhân
            var dieuTriList = await _context.DieuTriBenhNhans
                .Where(d => d.MaBenhNhan == chiPhi.MaBenhNhan)
                .Include(d => d.HinhThucDieuTri)
                .Include(d => d.BacSi)
                .ToListAsync();
                
            ViewBag.DieuTriList = dieuTriList;
            
            // Tính toán chi phí nếu bệnh nhân có bảo hiểm y tế (giảm 80%)
            if (chiPhi.BenhNhan != null && chiPhi.BenhNhan.BaoHiemYTe)
            {
                decimal chiPhiGoc = chiPhi.TongChiPhi;
                decimal giamGia = chiPhiGoc * 0.8m; // Giảm 80%
                decimal chiPhiSauGiamGia = chiPhiGoc * 0.2m; // Chỉ trả 20%
                
                ViewBag.ChiPhiGoc = chiPhiGoc;
                ViewBag.GiamGia = giamGia;
                ViewBag.ChiPhiSauGiamGia = chiPhiSauGiamGia;
                ViewBag.CoBaoHiem = true;
            }
            else
            {
                ViewBag.CoBaoHiem = false;
            }
            
            return View(chiPhi);
        }

        public async Task<IActionResult> Create(int? benhNhanId)
        {
            if (benhNhanId == null)
            {
                ViewBag.BenhNhanList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                    await _context.BenhNhans.ToListAsync(), "MaBenhNhan", "HoTen");
            }
            else
            {
                var benhNhan = await _context.BenhNhans.FindAsync(benhNhanId);
                if (benhNhan == null)
                {
                    return NotFound();
                }
                
                ViewBag.BenhNhan = benhNhan;
            }
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ChiPhiDieuTri chiPhiDieuTri)
        {
            if (ModelState.IsValid)
            {
                chiPhiDieuTri.NgayLap = DateTime.Now;
                _context.Add(chiPhiDieuTri);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            if (chiPhiDieuTri.MaBenhNhan == null)
            {
                ViewBag.BenhNhanList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                    await _context.BenhNhans.ToListAsync(), "MaBenhNhan", "HoTen");
            }
            else
            {
                var benhNhan = await _context.BenhNhans.FindAsync(chiPhiDieuTri.MaBenhNhan);
                ViewBag.BenhNhan = benhNhan;
            }
            
            return View(chiPhiDieuTri);
        }

        public async Task<IActionResult> ThanhToan(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chiPhi = await _context.ChiPhiDieuTris
                .Include(c => c.BenhNhan)
                .FirstOrDefaultAsync(c => c.MaChiPhi == id);
                
            if (chiPhi == null)
            {
                return NotFound();
            }

            if (chiPhi.DaThanhToan)
            {
                TempData["Message"] = "Chi phí này đã được thanh toán trước đó.";
                return RedirectToAction(nameof(Details), new { id });
            }
            
            // Tính toán chi phí nếu bệnh nhân có bảo hiểm y tế (giảm 80%)
            if (chiPhi.BenhNhan != null && chiPhi.BenhNhan.BaoHiemYTe)
            {
                decimal chiPhiGoc = chiPhi.TongChiPhi;
                decimal giamGia = chiPhiGoc * 0.8m; // Giảm 80%
                decimal chiPhiSauGiamGia = chiPhiGoc * 0.2m; // Chỉ trả 20%
                
                ViewBag.ChiPhiGoc = chiPhiGoc;
                ViewBag.GiamGia = giamGia;
                ViewBag.ChiPhiSauGiamGia = chiPhiSauGiamGia;
                ViewBag.CoBaoHiem = true;
            }
            else
            {
                ViewBag.CoBaoHiem = false;
            }
            
            return View(chiPhi);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThanhToanConfirmed(int id)
        {
            var chiPhi = await _context.ChiPhiDieuTris.FindAsync(id);
            
            if (chiPhi == null)
            {
                return NotFound();
            }

            chiPhi.DaThanhToan = true;
            _context.Update(chiPhi);
            await _context.SaveChangesAsync();
            
            TempData["Success"] = "Thanh toán thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}