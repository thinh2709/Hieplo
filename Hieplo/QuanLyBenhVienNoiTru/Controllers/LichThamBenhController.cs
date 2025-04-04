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
    public class LichThamBenhController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LichThamBenhController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var lichTham = await _context.LichThamBenhs
                .Include(l => l.KhachThamBenh)
                .Include(l => l.BenhNhan)
                .OrderByDescending(l => l.ThoiGianTham)
                .ToListAsync();
            return View(lichTham);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lichTham = await _context.LichThamBenhs
                .Include(l => l.KhachThamBenh)
                .Include(l => l.BenhNhan)
                .FirstOrDefaultAsync(m => m.MaLich == id);

            if (lichTham == null)
            {
                return NotFound();
            }

            return View(lichTham);
        }

        public async Task<IActionResult> Approve(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lichTham = await _context.LichThamBenhs
                .Include(l => l.KhachThamBenh)
                .Include(l => l.BenhNhan)
                .FirstOrDefaultAsync(m => m.MaLich == id);

            if (lichTham == null)
            {
                return NotFound();
            }

            return View(lichTham);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveConfirmed(int id)
        {
            var lichTham = await _context.LichThamBenhs.FindAsync(id);
            if (lichTham == null)
            {
                return NotFound();
            }

            lichTham.TrangThai = "Đã duyệt";
            _context.Update(lichTham);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Cancel(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lichTham = await _context.LichThamBenhs
                .Include(l => l.KhachThamBenh)
                .Include(l => l.BenhNhan)
                .FirstOrDefaultAsync(m => m.MaLich == id);

            if (lichTham == null)
            {
                return NotFound();
            }

            return View(lichTham);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id)
        {
            var lichTham = await _context.LichThamBenhs.FindAsync(id);
            if (lichTham == null)
            {
                return NotFound();
            }

            lichTham.TrangThai = "Hủy";
            _context.Update(lichTham);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }
    }
}