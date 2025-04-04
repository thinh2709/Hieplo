using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyBenhVienNoiTru.Data;
using QuanLyBenhVienNoiTru.Models;
using QuanLyBenhVienNoiTru.Models.ViewModels;
using System.Security.Claims;

namespace QuanLyBenhVienNoiTru.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.TaiKhoans
                    .FirstOrDefaultAsync(u => u.TenDangNhap == model.TenDangNhap && u.MatKhau == model.MatKhau);

                if (user != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.TenDangNhap),
                        new Claim(ClaimTypes.Role, user.VaiTro),
                        new Claim("UserId", user.MaTaiKhoan.ToString())
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    // Chuyển hướng dựa vào vai trò
                    switch (user.VaiTro)
                    {
                        case "Admin":
                            return RedirectToAction("Index", "Admin");
                        case "Bác sĩ":
                            return RedirectToAction("Index", "BacSi");
                        case "Khách":
                            return RedirectToAction("Index", "KhachThamBenh");
                        default:
                            return RedirectToAction("Index", "Home");
                    }
                }
                
                ModelState.AddModelError(string.Empty, "Đăng nhập không thành công");
            }
            
            return View(model);
        }
        
        [HttpGet]
        public async Task<IActionResult> Register()
        {
            // Lấy danh sách chuyên khoa từ bảng Khoa
            ViewBag.DanhSachKhoa = await _context.Khoas
                .Select(k => new SelectListItem
                {
                    Value = k.TenKhoa,
                    Text = k.TenKhoa
                })
                .ToListAsync();
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string GioiTinh)
        {
            // Lấy danh sách chuyên khoa từ bảng Khoa (để sử dụng nếu có lỗi và quay lại view)
            ViewBag.DanhSachKhoa = await _context.Khoas
                .Select(k => new SelectListItem
                {
                    Value = k.TenKhoa,
                    Text = k.TenKhoa
                })
                .ToListAsync();
                
            if (ModelState.IsValid)
            {
                // Kiểm tra tên đăng nhập đã tồn tại chưa
                if (await _context.TaiKhoans.AnyAsync(u => u.TenDangNhap == model.TenDangNhap))
                {
                    ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã tồn tại");
                    return View(model);
                }

                // Kiểm tra thông tin bắt buộc dựa trên vai trò
                if (model.VaiTro == "Bác sĩ")
                {
                    if (string.IsNullOrEmpty(model.HoTen))
                    {
                        ModelState.AddModelError("HoTen", "Vui lòng nhập họ tên bác sĩ");
                        return View(model);
                    }
                    if (string.IsNullOrEmpty(model.ChuyenKhoa))
                    {
                        ModelState.AddModelError("ChuyenKhoa", "Vui lòng chọn chuyên khoa");
                        return View(model);
                    }
                    if (string.IsNullOrEmpty(GioiTinh))
                    {
                        ModelState.AddModelError("GioiTinh", "Vui lòng chọn giới tính");
                        return View(model);
                    }
                }
                else if (model.VaiTro == "Khách")
                {
                    if (string.IsNullOrEmpty(model.HoTen))
                    {
                        ModelState.AddModelError("HoTen", "Vui lòng nhập họ tên khách thăm bệnh");
                        return View(model);
                    }
                    if (string.IsNullOrEmpty(model.MoiQuanHe))
                    {
                        ModelState.AddModelError("MoiQuanHe", "Vui lòng nhập mối quan hệ với bệnh nhân");
                        return View(model);
                    }
                }

                // Bắt đầu transaction
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Tạo tài khoản mới
                    var taiKhoan = new TaiKhoan
                    {
                        TenDangNhap = model.TenDangNhap,
                        MatKhau = model.MatKhau,
                        VaiTro = model.VaiTro
                    };

                    _context.TaiKhoans.Add(taiKhoan);
                    await _context.SaveChangesAsync();

                    // Tạo thông tin phụ thuộc vào vai trò
                    switch (model.VaiTro)
                    {
                        case "Bác sĩ":
                            var bacSi = new BacSi
                            {
                                HoTen = model.HoTen,
                                MaTaiKhoan = taiKhoan.MaTaiKhoan,
                                ChuyenKhoa = model.ChuyenKhoa,
                                SoDienThoai = model.SoDienThoai,
                                GioiTinh = GioiTinh
                            };
                            _context.BacSis.Add(bacSi);
                            break;
                        case "Khách":
                            var khach = new KhachThamBenh
                            {
                                HoTen = model.HoTen,
                                MaTaiKhoan = taiKhoan.MaTaiKhoan,
                                SoDienThoai = model.SoDienThoai,
                                MoiQuanHe = model.MoiQuanHe
                            };
                            _context.KhachThamBenhs.Add(khach);
                            break;
                        case "Admin":
                            // Không cần thêm thông tin bổ sung cho Admin
                            break;
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Thông báo thành công và chuyển đến trang đăng nhập
                    TempData["SuccessMessage"] = "Đăng ký tài khoản thành công!";
                    return RedirectToAction("Login");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError(string.Empty, "Đăng ký không thành công: " + ex.Message);
                }
            }
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}