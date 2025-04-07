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
        private readonly ILogger<AccountController> _logger;

        public AccountController(ApplicationDbContext context, ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
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
            try
            {
                // Log thông tin đăng nhập để debug
                _logger.LogInformation("Thông tin đăng nhập - TenDangNhap: {TenDangNhap}", model.TenDangNhap);

                // Kiểm tra ModelState trước khi xử lý
                ModelState.Remove("Email"); // Xóa bất kỳ lỗi nào liên quan đến Email
                
                if (ModelState.IsValid)
                {
                    // Log truy vấn database
                    _logger.LogInformation("Đang tìm kiếm tài khoản trong database...");
                    
                    // Tìm tài khoản theo tên đăng nhập
                    var user = await _context.TaiKhoans
                        .FirstOrDefaultAsync(u => u.TenDangNhap == model.TenDangNhap);

                    if (user != null)
                    {
                        // So sánh mật khẩu
                        if (user.MatKhau == model.MatKhau) // Tạm thời so sánh trực tiếp, sau này sẽ thêm mã hóa
                        {
                            _logger.LogInformation("Tìm thấy tài khoản: {TenDangNhap}, Vai trò: {VaiTro}", user.TenDangNhap, user.VaiTro);
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

                            _logger.LogInformation("Đăng nhập thành công cho người dùng {TenDangNhap}", user.TenDangNhap);

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
                    }
                    
                    _logger.LogWarning("Đăng nhập thất bại cho tên đăng nhập {TenDangNhap}", model.TenDangNhap);
                    ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng");
                }
                else
                {
                    _logger.LogWarning("ModelState không hợp lệ - Lỗi: {Errors}", 
                        string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đăng nhập: {Message}", ex.Message);
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi đăng nhập. Vui lòng thử lại sau.");
            }
            
            return View(model);
        }
        
        [HttpGet]
        public async Task<IActionResult> Register()
        {
            try
            {
                var khoas = await _context.Khoas
                    .Where(k => k.TrangThai) // Chỉ lấy các khoa đang hoạt động
                    .OrderBy(k => k.TenKhoa)
                    .Select(k => new SelectListItem
                    {
                        Value = k.MaKhoa.ToString(),
                        Text = k.TenKhoa
                    })
                    .ToListAsync();

                if (!khoas.Any())
                {
                    _logger.LogWarning("Không có khoa nào đang hoạt động trong hệ thống");
                    TempData["ErrorMessage"] = "Hệ thống chưa có khoa nào đang hoạt động. Vui lòng liên hệ quản trị viên.";
                    return RedirectToAction("Index", "Home");
                }

                ViewBag.DanhSachKhoaID = khoas;
                ViewBag.GioiTinhList = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Nam", Text = "Nam" },
                    new SelectListItem { Value = "Nữ", Text = "Nữ" },
                    new SelectListItem { Value = "Khác", Text = "Khác" }
                };

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải trang đăng ký: {Message}", ex.Message);
                TempData["ErrorMessage"] = "Không thể tải trang đăng ký. Vui lòng thử lại sau.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            try
            {
                // Lấy danh sách khoa để hiển thị lại nếu có lỗi
                var khoas = await _context.Khoas
                    .Where(k => k.TrangThai)
                    .OrderBy(k => k.TenKhoa)
                    .Select(k => new SelectListItem
                    {
                        Value = k.MaKhoa.ToString(),
                        Text = k.TenKhoa
                    })
                    .ToListAsync();

                ViewBag.DanhSachKhoaID = khoas;
                ViewBag.GioiTinhList = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Nam", Text = "Nam" },
                    new SelectListItem { Value = "Nữ", Text = "Nữ" },
                    new SelectListItem { Value = "Khác", Text = "Khác" }
                };

                // Log thông tin đăng ký
                _logger.LogInformation("Bắt đầu đăng ký tài khoản cho {TenDangNhap} với vai trò {VaiTro}", 
                    model.TenDangNhap, model.VaiTro);

                // Xóa các ModelState error không liên quan đến vai trò
                if (model.VaiTro == "Khách")
                {
                    ModelState.Remove("GioiTinh");
                    ModelState.Remove("MaKhoa");
                    ModelState.Remove("ChuyenKhoa");
                    
                    if (string.IsNullOrWhiteSpace(model.MoiQuanHe))
                    {
                        ModelState.AddModelError("MoiQuanHe", "Vui lòng nhập mối quan hệ");
                    }
                }
                else if (model.VaiTro == "Bác sĩ")
                {
                    ModelState.Remove("MoiQuanHe");
                    
                    if (string.IsNullOrWhiteSpace(model.GioiTinh))
                    {
                        ModelState.AddModelError("GioiTinh", "Vui lòng chọn giới tính");
                    }
                    if (model.MaKhoa == 0)
                    {
                        ModelState.AddModelError("MaKhoa", "Vui lòng chọn khoa");
                    }
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                        .Select(e => e.ErrorMessage);
                    _logger.LogWarning("Lỗi validation: {Errors}", string.Join(", ", errors));
                    return View(model);
                }

                // Kiểm tra tên đăng nhập đã tồn tại
                var existingUser = await _context.TaiKhoans
                    .FirstOrDefaultAsync(u => u.TenDangNhap == model.TenDangNhap);
                if (existingUser != null)
                {
                    _logger.LogWarning("Tên đăng nhập {TenDangNhap} đã tồn tại", model.TenDangNhap);
                    ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã tồn tại");
                    return View(model);
                }

                // Kiểm tra email đã tồn tại
                var existingEmail = model.VaiTro == "Bác sĩ" ?
                    await _context.BacSis.AnyAsync(b => b.Email == model.Email) :
                    await _context.KhachThamBenhs.AnyAsync(k => k.Email == model.Email);
                if (existingEmail)
                {
                    _logger.LogWarning("Email {Email} đã tồn tại", model.Email);
                    ModelState.AddModelError("Email", "Email đã được sử dụng");
                    return View(model);
                }

                // Kiểm tra số điện thoại đã tồn tại
                var existingPhone = model.VaiTro == "Bác sĩ" ?
                    await _context.BacSis.AnyAsync(b => b.SoDienThoai == model.SoDienThoai) :
                    await _context.KhachThamBenhs.AnyAsync(k => k.SoDienThoai == model.SoDienThoai);
                if (existingPhone)
                {
                    _logger.LogWarning("Số điện thoại {SoDienThoai} đã tồn tại", model.SoDienThoai);
                    ModelState.AddModelError("SoDienThoai", "Số điện thoại đã được sử dụng");
                    return View(model);
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
                    _logger.LogInformation("Đã tạo tài khoản {MaTaiKhoan}", taiKhoan.MaTaiKhoan);

                    // Tạo thông tin phụ thuộc vào vai trò
                    if (model.VaiTro == "Bác sĩ")
                    {
                        if (model.MaKhoa <= 0)
                        {
                            throw new Exception("Vui lòng chọn khoa");
                        }

                        var khoa = await _context.Khoas.FindAsync(model.MaKhoa);
                        if (khoa == null)
                        {
                            throw new Exception("Khoa không tồn tại");
                        }

                        var bacSi = new BacSi
                        {
                            MaTaiKhoan = taiKhoan.MaTaiKhoan,
                            HoTen = model.HoTen,
                            ChuyenKhoa = khoa.TenKhoa, // Sử dụng tên khoa làm chuyên khoa
                            SoDienThoai = model.SoDienThoai,
                            Email = model.Email,
                            DiaChi = model.DiaChi,
                            GioiTinh = model.GioiTinh,
                            NgayVaoLam = DateTime.Now,
                            TrangThai = true,
                            MaKhoa = model.MaKhoa
                        };
                        _context.BacSis.Add(bacSi);
                        _logger.LogInformation("Đã tạo thông tin bác sĩ cho tài khoản {MaTaiKhoan}", taiKhoan.MaTaiKhoan);
                    }
                    else if (model.VaiTro == "Khách")
                    {
                        if (string.IsNullOrEmpty(model.MoiQuanHe))
                        {
                            throw new Exception("Vui lòng nhập mối quan hệ");
                        }

                        var khach = new KhachThamBenh
                        {
                            MaTaiKhoan = taiKhoan.MaTaiKhoan,
                            HoTen = model.HoTen,
                            SoDienThoai = model.SoDienThoai,
                            Email = model.Email,
                            DiaChi = model.DiaChi,
                            MoiQuanHe = model.MoiQuanHe
                        };
                        _context.KhachThamBenhs.Add(khach);
                        _logger.LogInformation("Đã tạo thông tin khách thăm bệnh cho tài khoản {MaTaiKhoan}", taiKhoan.MaTaiKhoan);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Đăng ký thành công cho {TenDangNhap}", model.TenDangNhap);
                    TempData["SuccessMessage"] = "Đăng ký tài khoản thành công!";
                    return RedirectToAction("Login");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Lỗi khi đăng ký tài khoản: {Message}", ex.Message);

                    // Xử lý các lỗi cụ thể
                    if (ex.InnerException != null)
                    {
                        if (ex.InnerException.Message.Contains("FK_BacSis_Khoas"))
                        {
                            ModelState.AddModelError("MaKhoa", "Khoa không tồn tại trong hệ thống");
                        }
                        else if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã tồn tại");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Đăng ký không thành công: " + ex.InnerException.Message);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Đăng ký không thành công: " + ex.Message);
                    }

                    // Log chi tiết lỗi
                    _logger.LogError("Chi tiết lỗi: {@Error}", new
                    {
                        ExceptionType = ex.GetType().Name,
                        ex.Message,
                        ex.StackTrace,
                        InnerException = ex.InnerException?.Message,
                        ModelState = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                    });

                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi không xác định khi đăng ký: {Message}", ex.Message);
                TempData["ErrorMessage"] = "Có lỗi xảy ra trong quá trình đăng ký. Vui lòng thử lại sau.";
                return View(model);
            }
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

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            try
            {
                // Lấy thông tin người dùng đăng nhập
                var userId = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login");
                }

                // Chuyển về số nguyên
                if (!int.TryParse(userId, out int maTaiKhoan))
                {
                    return NotFound("Không tìm thấy thông tin người dùng");
                }

                // Lấy thông tin tài khoản
                var taiKhoan = await _context.TaiKhoans
                    .FirstOrDefaultAsync(t => t.MaTaiKhoan == maTaiKhoan);

                if (taiKhoan == null)
                {
                    return NotFound("Không tìm thấy thông tin tài khoản");
                }

                // Tạo model thông tin profile dựa vào vai trò
                var profileModel = new ProfileViewModel
                {
                    MaTaiKhoan = taiKhoan.MaTaiKhoan,
                    TenDangNhap = taiKhoan.TenDangNhap,
                    VaiTro = taiKhoan.VaiTro
                };

                // Nếu là bác sĩ, lấy thêm thông tin từ bảng BacSi
                if (taiKhoan.VaiTro == "Bác sĩ")
                {
                    var bacSi = await _context.BacSis
                        .Include(b => b.Khoa)
                        .FirstOrDefaultAsync(b => b.MaTaiKhoan == maTaiKhoan);

                    if (bacSi != null)
                    {
                        profileModel.HoTen = bacSi.HoTen;
                        profileModel.Email = bacSi.Email;
                        profileModel.SoDienThoai = bacSi.SoDienThoai;
                        profileModel.DiaChi = bacSi.DiaChi;
                        profileModel.GioiTinh = bacSi.GioiTinh;
                        // Ở đây BacSi không có NgaySinh, nên chúng ta không thể gán giá trị này
                        profileModel.ChuyenKhoa = bacSi.ChuyenKhoa;
                        profileModel.MaKhoa = bacSi.MaKhoa;
                        profileModel.TenKhoa = bacSi.Khoa?.TenKhoa;
                        profileModel.NgayVaoLam = bacSi.NgayVaoLam;
                    }
                }
                // Nếu là khách, lấy thêm thông tin từ bảng KhachThamBenh
                else if (taiKhoan.VaiTro == "Khách")
                {
                    var khach = await _context.KhachThamBenhs
                        .FirstOrDefaultAsync(k => k.MaTaiKhoan == maTaiKhoan);

                    if (khach != null)
                    {
                        profileModel.HoTen = khach.HoTen;
                        profileModel.Email = khach.Email;
                        profileModel.SoDienThoai = khach.SoDienThoai;
                        profileModel.DiaChi = khach.DiaChi;
                        profileModel.MoiQuanHe = khach.MoiQuanHe;
                    }
                }
                // Nếu là Admin, tạm thời chỉ hiển thị các thông tin cơ bản
                else if (taiKhoan.VaiTro == "Admin")
                {
                    // Đối với Admin, có thể cần thêm bảng dữ liệu riêng hoặc chỉ hiển thị thông tin cơ bản
                    profileModel.HoTen = "Admin"; // Giá trị mặc định
                }

                // Lấy danh sách khoa nếu cần hiển thị trong dropdown
                ViewBag.DanhSachKhoa = await _context.Khoas
                    .Where(k => k.TrangThai)
                    .Select(k => new SelectListItem
                    {
                        Value = k.MaKhoa.ToString(),
                        Text = k.TenKhoa
                    })
                    .ToListAsync();

                ViewBag.GioiTinhList = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Nam", Text = "Nam" },
                    new SelectListItem { Value = "Nữ", Text = "Nữ" },
                    new SelectListItem { Value = "Khác", Text = "Khác" }
                };

                return View(profileModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải trang hồ sơ cá nhân: {Message}", ex.Message);
                TempData["ErrorMessage"] = "Không thể tải thông tin hồ sơ cá nhân. Vui lòng thử lại sau.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            try
            {
                // Lấy thông tin người dùng đăng nhập để kiểm tra
                var userId = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int maTaiKhoan) || maTaiKhoan != model.MaTaiKhoan)
                {
                    return Unauthorized("Bạn không có quyền cập nhật thông tin này");
                }

                // Lấy thông tin tài khoản hiện tại
                var taiKhoan = await _context.TaiKhoans
                    .FirstOrDefaultAsync(t => t.MaTaiKhoan == maTaiKhoan);

                if (taiKhoan == null)
                {
                    return NotFound("Không tìm thấy thông tin tài khoản");
                }

                // Nếu có thay đổi mật khẩu
                if (!string.IsNullOrEmpty(model.MatKhauMoi) && !string.IsNullOrEmpty(model.XacNhanMatKhau))
                {
                    if (model.MatKhauMoi != model.XacNhanMatKhau)
                    {
                        ModelState.AddModelError("XacNhanMatKhau", "Mật khẩu xác nhận không khớp với mật khẩu mới");
                        
                        // Lấy lại danh sách khoa và giới tính cho view
                        ViewBag.DanhSachKhoa = await _context.Khoas
                            .Where(k => k.TrangThai)
                            .Select(k => new SelectListItem
                            {
                                Value = k.MaKhoa.ToString(),
                                Text = k.TenKhoa
                            })
                            .ToListAsync();

                        ViewBag.GioiTinhList = new List<SelectListItem>
                        {
                            new SelectListItem { Value = "Nam", Text = "Nam" },
                            new SelectListItem { Value = "Nữ", Text = "Nữ" },
                            new SelectListItem { Value = "Khác", Text = "Khác" }
                        };
                        
                        return View(model);
                    }

                    // Cập nhật mật khẩu mới (trong thực tế nên có mã hóa)
                    taiKhoan.MatKhau = model.MatKhauMoi;
                }

                // Cập nhật thông tin theo vai trò
                if (taiKhoan.VaiTro == "Bác sĩ")
                {
                    var bacSi = await _context.BacSis
                        .FirstOrDefaultAsync(b => b.MaTaiKhoan == maTaiKhoan);

                    if (bacSi != null)
                    {
                        // Cập nhật thông tin từ model vào đối tượng BacSi
                        bacSi.HoTen = model.HoTen;
                        bacSi.Email = model.Email;
                        bacSi.SoDienThoai = model.SoDienThoai;
                        bacSi.DiaChi = model.DiaChi;
                        bacSi.GioiTinh = model.GioiTinh;
                        // Không cần cập nhật NgaySinh vì BacSi không có thuộc tính này
                        
                        _context.Update(bacSi);
                    }
                }
                else if (taiKhoan.VaiTro == "Khách")
                {
                    var khach = await _context.KhachThamBenhs
                        .FirstOrDefaultAsync(k => k.MaTaiKhoan == maTaiKhoan);

                    if (khach != null)
                    {
                        // Cập nhật thông tin từ model vào đối tượng KhachThamBenh
                        khach.HoTen = model.HoTen;
                        khach.Email = model.Email;
                        khach.SoDienThoai = model.SoDienThoai;
                        khach.DiaChi = model.DiaChi;
                        khach.MoiQuanHe = model.MoiQuanHe;
                        
                        _context.Update(khach);
                    }
                }

                _context.Update(taiKhoan);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cập nhật thông tin cá nhân thành công!";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật hồ sơ cá nhân: {Message}", ex.Message);
                ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật thông tin. Vui lòng thử lại sau.");
                
                // Lấy lại danh sách khoa và giới tính cho view
                ViewBag.DanhSachKhoa = await _context.Khoas
                    .Where(k => k.TrangThai)
                    .Select(k => new SelectListItem
                    {
                        Value = k.MaKhoa.ToString(),
                        Text = k.TenKhoa
                    })
                    .ToListAsync();

                ViewBag.GioiTinhList = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Nam", Text = "Nam" },
                    new SelectListItem { Value = "Nữ", Text = "Nữ" },
                    new SelectListItem { Value = "Khác", Text = "Khác" }
                };
                
                return View(model);
            }
        }
    }
}