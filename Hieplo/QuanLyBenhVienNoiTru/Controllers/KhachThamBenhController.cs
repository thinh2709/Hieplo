using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBenhVienNoiTru.Data;
using QuanLyBenhVienNoiTru.Models;
using QuanLyBenhVienNoiTru.Filters;
using QuanLyBenhVienNoiTru.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace QuanLyBenhVienNoiTru.Controllers
{
    [AuthorizeRoles("Khách")]
    public class KhachThamBenhController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KhachThamBenhController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue("UserId"));
            var khach = await _context.KhachThamBenhs
                .FirstOrDefaultAsync(k => k.MaTaiKhoan == userId);

            if (khach == null)
            {
                return NotFound();
            }

            return View(khach);
        }

        public async Task<IActionResult> DangKyTham()
        {
            var userId = int.Parse(User.FindFirstValue("UserId"));
            var khach = await _context.KhachThamBenhs
                .FirstOrDefaultAsync(k => k.MaTaiKhoan == userId);

            if (khach == null)
            {
                return NotFound();
            }

            var benhNhans = await _context.BenhNhans
                .Where(b => b.NgayXuatVien == null)
                .ToListAsync();

            var model = new DangKyThamViewModel
            {
                MaKhach = khach.MaKhach,
                BenhNhans = benhNhans,
                ThoiGianTham = DateTime.Now.AddDays(1)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKyTham(DangKyThamViewModel model)
        {
            if (ModelState.IsValid)
            {
                var lichTham = new LichThamBenh
                {
                    MaKhach = model.MaKhach,
                    MaBenhNhan = model.MaBenhNhan,
                    ThoiGianTham = model.ThoiGianTham,
                    TrangThai = "Chờ duyệt"
                };

                _context.LichThamBenhs.Add(lichTham);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(LichSuTham));
            }

            model.BenhNhans = await _context.BenhNhans
                .Where(b => b.NgayXuatVien == null)
                .ToListAsync();

            return View(model);
        }

        public async Task<IActionResult> LichSuTham()
        {
            var userId = int.Parse(User.FindFirstValue("UserId"));
            var khach = await _context.KhachThamBenhs
                .FirstOrDefaultAsync(k => k.MaTaiKhoan == userId);

            if (khach == null)
            {
                return NotFound();
            }

            var lichTham = await _context.LichThamBenhs
                .Include(l => l.BenhNhan)
                .Where(l => l.MaKhach == khach.MaKhach)
                .OrderByDescending(l => l.ThoiGianTham)
                .ToListAsync();

            return View(lichTham);
        }
    }
}