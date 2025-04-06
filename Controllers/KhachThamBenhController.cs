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
    [AuthorizeRoles("Khách", "Admin", "Bác sĩ")]
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
            try 
            {
                var userId = int.Parse(User.FindFirstValue("UserId"));
                var khach = await _context.KhachThamBenhs
                    .FirstOrDefaultAsync(k => k.MaTaiKhoan == userId);

                if (khach == null)
                {
                    TempData["Error"] = "Không tìm thấy thông tin khách thăm bệnh. Vui lòng đăng nhập lại.";
                    return RedirectToAction("Index", "Home");
                }

                var benhNhans = await _context.BenhNhans
                    .Where(b => b.NgayXuatVien == null)
                    .ToListAsync();
                
                if (benhNhans == null || !benhNhans.Any())
                {
                    TempData["Warning"] = "Hiện không có bệnh nhân nào đang nội trú. Vui lòng thử lại sau.";
                }

                var model = new DangKyThamViewModel
                {
                    MaKhach = khach.MaKhach,
                    BenhNhans = benhNhans ?? new List<BenhNhan>(),
                    ThoiGianTham = DateTime.Now.AddDays(1)
                };

                // Log that we're returning the model with BenhNhans
                System.Diagnostics.Debug.WriteLine($"GET DangKyTham - MaKhach: {model.MaKhach}, BenhNhans count: {model.BenhNhans.Count()}");
                
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Có lỗi xảy ra: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKyTham(DangKyThamViewModel model)
        {
            // Ensure BenhNhans is always populated
            model.BenhNhans = await _context.BenhNhans
                .Where(b => b.NgayXuatVien == null)
                .ToListAsync();
            
            // Remove validation errors for BenhNhans if they exist
            ModelState.Remove("BenhNhans");
                
            // Log the model state for debugging
            if (!ModelState.IsValid)
            {
                var errorMessages = string.Join("; ", 
                    ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                
                TempData["Error"] = $"Lỗi xác thực dữ liệu: {errorMessages}";
                return View(model);
            }
            
            try
            {
                // Ensure MaKhach is valid
                var khach = await _context.KhachThamBenhs.FindAsync(model.MaKhach);
                if (khach == null)
                {
                    TempData["Error"] = "Không tìm thấy thông tin khách thăm bệnh. Vui lòng đăng nhập lại.";
                    return RedirectToAction("Index", "Home");
                }
                
                // Ensure MaBenhNhan is valid
                var benhNhan = await _context.BenhNhans.FindAsync(model.MaBenhNhan);
                if (benhNhan == null)
                {
                    ModelState.AddModelError("MaBenhNhan", "Bệnh nhân không tồn tại");
                    return View(model);
                }
                
                // Check if patient has been discharged
                if (benhNhan.NgayXuatVien.HasValue)
                {
                    ModelState.AddModelError("MaBenhNhan", "Bệnh nhân này đã xuất viện, không thể đăng ký thăm");
                    return View(model);
                }
                
                // Kiểm tra thời gian thăm
                if (model.ThoiGianTham <= DateTime.Now)
                {
                    ModelState.AddModelError("ThoiGianTham", "Thời gian thăm phải lớn hơn thời gian hiện tại");
                    return View(model);
                }

                // Log the data being saved
                System.Diagnostics.Debug.WriteLine($"Saving visit: MaKhach={model.MaKhach}, MaBenhNhan={model.MaBenhNhan}, ThoiGianTham={model.ThoiGianTham}");
                
                var lichTham = new LichThamBenh
                {
                    MaKhach = model.MaKhach,
                    MaBenhNhan = model.MaBenhNhan,
                    ThoiGianTham = model.ThoiGianTham,
                    TrangThai = "Chờ duyệt"
                };

                _context.LichThamBenhs.Add(lichTham);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Đăng ký thăm bệnh thành công. Vui lòng chờ phê duyệt từ bệnh viện.";
                return RedirectToAction(nameof(LichSuTham));
            }
            catch (Exception ex)
            {
                // Detailed error logging
                System.Diagnostics.Debug.WriteLine($"ERROR in DangKyTham: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                
                ModelState.AddModelError("", $"Lỗi hệ thống: {ex.Message}");
                TempData["Error"] = $"Không thể đăng ký lịch thăm bệnh: {ex.Message}";
                return View(model);
            }
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

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            KhachThamBenh khach;
            
            if (User.IsInRole("Admin") || User.IsInRole("Bác sĩ"))
            {
                khach = await _context.KhachThamBenhs
                    .Include(k => k.LichThamBenhs)
                        .ThenInclude(l => l.BenhNhan)
                    .FirstOrDefaultAsync(k => k.MaKhach == id);
            }
            else
            {
                var userId = int.Parse(User.FindFirstValue("UserId"));
                khach = await _context.KhachThamBenhs
                    .Include(k => k.LichThamBenhs)
                        .ThenInclude(l => l.BenhNhan)
                    .FirstOrDefaultAsync(k => k.MaKhach == id && k.MaTaiKhoan == userId);
            }

            if (khach == null)
            {
                return NotFound();
            }

            return View(khach);
        }
    }
}