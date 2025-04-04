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
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TongBenhNhan = await _context.BenhNhans.CountAsync();
            ViewBag.BenhNhanDangDieuTri = await _context.BenhNhans.Where(b => b.NgayXuatVien == null).CountAsync();
            ViewBag.TongBacSi = await _context.BacSis.CountAsync();
            ViewBag.TongKhoa = await _context.Khoas.CountAsync();
            
            return View();
        }

        public async Task<IActionResult> ManageUsers()
        {
            var users = await _context.TaiKhoans.ToListAsync();
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _context.TaiKhoans.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int id, TaiKhoan taiKhoan)
        {
            if (id != taiKhoan.MaTaiKhoan)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(taiKhoan);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.TaiKhoans.AnyAsync(e => e.MaTaiKhoan == id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(ManageUsers));
            }
            
            return View(taiKhoan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.TaiKhoans.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.TaiKhoans.Remove(user);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(ManageUsers));
        }
    }
}