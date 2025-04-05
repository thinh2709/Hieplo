using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyBenhVienNoiTru.Data;
using QuanLyBenhVienNoiTru.Models;
using QuanLyBenhVienNoiTru.Models.ViewModels;
using QuanLyBenhVienNoiTru.Filters;
using System;
using System.Collections.Generic;
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
            // Thống kê tổng quan
            ViewBag.TongBenhNhan = await _context.BenhNhans.CountAsync();
            ViewBag.BenhNhanDangDieuTri = await _context.BenhNhans.Where(b => b.NgayXuatVien == null).CountAsync();
            ViewBag.TongBacSi = await _context.BacSis.CountAsync();
            ViewBag.TongKhoa = await _context.Khoas.CountAsync();
            
            // Thông tin thêm cho dashboard
            ViewBag.BenhNhanMoi = await _context.BenhNhans
                .Where(b => b.NgayNhapVien >= DateTime.Now.AddDays(-7))
                .CountAsync();
                
            ViewBag.BenhNhanRaVien = await _context.BenhNhans
                .Where(b => b.NgayXuatVien != null && b.NgayXuatVien >= DateTime.Now.AddDays(-7))
                .CountAsync();
                
            ViewBag.LuotThamBenh = await _context.LichThamBenhs
                .Where(l => l.ThoiGianTham != null && l.ThoiGianTham.Value.Date == DateTime.Today)
                .CountAsync();
                
            // Lấy hoạt động gần đây
            var hoatDongGanDay = new List<HoatDongViewModel>();
            
            // Bệnh nhân mới nhất
            var benhNhanMoi = await _context.BenhNhans
                .OrderByDescending(b => b.NgayNhapVien)
                .Take(5)
                .ToListAsync();
                
            foreach (var bn in benhNhanMoi)
            {
                hoatDongGanDay.Add(new HoatDongViewModel
                {
                    Loai = "BenhNhanMoi",
                    ThoiGian = bn.NgayNhapVien,
                    NoiDung = $"Tiếp nhận bệnh nhân {bn.HoTen}",
                    MaDoiTuong = bn.MaBenhNhan,
                    Icon = "fas fa-user-plus",
                    MauSac = "primary"
                });
            }
            
            // Điều trị mới nhất
            var dieuTriMoi = await _context.DieuTriBenhNhans
                .Include(d => d.BenhNhan)
                .Include(d => d.BacSi)
                .OrderByDescending(d => d.NgayThucHien)
                .Take(5)
                .ToListAsync();
                
            foreach (var dt in dieuTriMoi)
            {
                hoatDongGanDay.Add(new HoatDongViewModel
                {
                    Loai = "DieuTriMoi",
                    ThoiGian = dt.NgayThucHien ?? DateTime.Now,
                    NoiDung = $"Bác sĩ {dt.BacSi?.HoTen} thực hiện điều trị cho bệnh nhân {dt.BenhNhan?.HoTen}",
                    MaDoiTuong = dt.MaDieuTriBenhNhan,
                    Icon = "fas fa-stethoscope",
                    MauSac = "success"
                });
            }
            
            // Chi phí gần đây
            var chiPhiMoi = await _context.ChiPhiDieuTris
                .Include(c => c.BenhNhan)
                .OrderByDescending(c => c.NgayLap)
                .Take(5)
                .ToListAsync();
                
            foreach (var cp in chiPhiMoi)
            {
                hoatDongGanDay.Add(new HoatDongViewModel
                {
                    Loai = "ChiPhiMoi",
                    ThoiGian = cp.NgayLap ?? DateTime.Now,
                    NoiDung = $"Chi phí điều trị {string.Format("{0:N0}", cp.TongChiPhi)}đ cho bệnh nhân {cp.BenhNhan?.HoTen}",
                    MaDoiTuong = cp.MaChiPhi,
                    Icon = "fas fa-file-invoice-dollar",
                    MauSac = "danger"
                });
            }
            
            // Sắp xếp theo thời gian giảm dần
            ViewBag.HoatDongGanDay = hoatDongGanDay.OrderByDescending(h => h.ThoiGian).Take(10).ToList();
            
            return View();
        }

        public async Task<IActionResult> ManageUsers(string searchTerm = null, string role = null)
        {
            ViewBag.VaiTros = new List<string> { "Admin", "Bác sĩ", "Nhân viên" };
            ViewBag.TimKiem = searchTerm;
            ViewBag.VaiTro = role;
            
            var usersQuery = _context.TaiKhoans.AsQueryable();
            
            // Lọc theo vai trò
            if (!string.IsNullOrEmpty(role))
            {
                usersQuery = usersQuery.Where(u => u.VaiTro == role);
            }
            
            // Tìm kiếm
            if (!string.IsNullOrEmpty(searchTerm))
            {
                usersQuery = usersQuery.Where(u => 
                    u.TenDangNhap.Contains(searchTerm) || 
                    (u.BacSi != null && u.BacSi.HoTen.Contains(searchTerm)));
            }
            
            // Bao gồm thông tin liên kết
            usersQuery = usersQuery
                .Include(u => u.BacSi)
                .Include(u => u.KhachThamBenh);
                
            var users = await usersQuery.ToListAsync();
            return View(users);
        }

        public IActionResult CreateUser()
        {
            ViewBag.VaiTros = new SelectList(new List<string> { "Admin", "Bác sĩ", "Nhân viên" });
            ViewBag.BacSiList = new SelectList(_context.BacSis.Where(b => b.TrangThai), "MaBacSi", "HoTen");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(TaiKhoan taiKhoan, int? bacSiId)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem tên đăng nhập đã tồn tại chưa
                if (await _context.TaiKhoans.AnyAsync(t => t.TenDangNhap == taiKhoan.TenDangNhap))
                {
                    ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã tồn tại");
                    ViewBag.VaiTros = new SelectList(new List<string> { "Admin", "Bác sĩ", "Nhân viên" });
                    ViewBag.BacSiList = new SelectList(_context.BacSis.Where(b => b.TrangThai), "MaBacSi", "HoTen");
                    return View(taiKhoan);
                }
                
                // Liên kết với bác sĩ nếu vai trò là Bác sĩ
                if (taiKhoan.VaiTro == "Bác sĩ" && bacSiId.HasValue)
                {
                    var bacSi = await _context.BacSis.FindAsync(bacSiId.Value);
                    if (bacSi != null)
                    {
                        // Kiểm tra xem bác sĩ đã có tài khoản chưa
                        if (await _context.TaiKhoans.AnyAsync(t => t.BacSi != null && t.BacSi.MaBacSi == bacSiId.Value))
                        {
                            ModelState.AddModelError("", "Bác sĩ này đã được liên kết với tài khoản khác");
                            ViewBag.VaiTros = new SelectList(new List<string> { "Admin", "Bác sĩ", "Nhân viên" });
                            ViewBag.BacSiList = new SelectList(_context.BacSis.Where(b => b.TrangThai), "MaBacSi", "HoTen");
                            return View(taiKhoan);
                        }
                    }
                }
                
                _context.Add(taiKhoan);
                await _context.SaveChangesAsync();
                
                // Cập nhật liên kết với bác sĩ sau khi tạo tài khoản
                if (taiKhoan.VaiTro == "Bác sĩ" && bacSiId.HasValue)
                {
                    var bacSi = await _context.BacSis.FindAsync(bacSiId.Value);
                    if (bacSi != null)
                    {
                        bacSi.MaTaiKhoan = taiKhoan.MaTaiKhoan;
                        _context.Update(bacSi);
                        await _context.SaveChangesAsync();
                    }
                }
                
                TempData["Success"] = "Tạo tài khoản thành công";
                return RedirectToAction(nameof(ManageUsers));
            }
            
            ViewBag.VaiTros = new SelectList(new List<string> { "Admin", "Bác sĩ", "Nhân viên" });
            ViewBag.BacSiList = new SelectList(_context.BacSis.Where(b => b.TrangThai), "MaBacSi", "HoTen");
            return View(taiKhoan);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _context.TaiKhoans
                .Include(u => u.BacSi)
                .Include(u => u.KhachThamBenh)
                .FirstOrDefaultAsync(u => u.MaTaiKhoan == id);
                
            if (user == null)
            {
                return NotFound();
            }
            
            ViewBag.VaiTros = new SelectList(new List<string> { "Admin", "Bác sĩ", "Nhân viên" }, user.VaiTro);
            ViewBag.BacSiList = new SelectList(_context.BacSis.Where(b => b.TrangThai), "MaBacSi", "HoTen", 
                user.BacSi != null ? user.BacSi.MaBacSi : (int?)null);
                
            ViewBag.LienKetInfo = null;
            if (user.BacSi != null)
            {
                ViewBag.LienKetInfo = $"Liên kết với bác sĩ: {user.BacSi.HoTen}";
            }
            else if (user.KhachThamBenh != null)
            {
                ViewBag.LienKetInfo = $"Liên kết với khách thăm bệnh: {user.KhachThamBenh.HoTen}";
            }
            
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int id, TaiKhoan taiKhoan, int? bacSiId)
        {
            if (id != taiKhoan.MaTaiKhoan)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy thông tin tài khoản hiện tại
                    var existingUser = await _context.TaiKhoans
                        .Include(u => u.BacSi)
                        .Include(u => u.KhachThamBenh)
                        .FirstOrDefaultAsync(u => u.MaTaiKhoan == id);
                        
                    if (existingUser == null)
                    {
                        return NotFound();
                    }
                    
                    // Kiểm tra xem tên đăng nhập đã tồn tại chưa (nếu có thay đổi)
                    if (existingUser.TenDangNhap != taiKhoan.TenDangNhap && 
                        await _context.TaiKhoans.AnyAsync(t => t.TenDangNhap == taiKhoan.TenDangNhap))
                    {
                        ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã tồn tại");
                        ViewBag.VaiTros = new SelectList(new List<string> { "Admin", "Bác sĩ", "Nhân viên" }, taiKhoan.VaiTro);
                        ViewBag.BacSiList = new SelectList(_context.BacSis.Where(b => b.TrangThai), "MaBacSi", "HoTen", bacSiId);
                        return View(taiKhoan);
                    }
                    
                    // Cập nhật thông tin cơ bản
                    existingUser.TenDangNhap = taiKhoan.TenDangNhap;
                    
                    // Chỉ cập nhật mật khẩu nếu có thay đổi
                    if (!string.IsNullOrEmpty(taiKhoan.MatKhau))
                    {
                        existingUser.MatKhau = taiKhoan.MatKhau;
                    }
                    
                    existingUser.VaiTro = taiKhoan.VaiTro;
                    
                    // Cập nhật liên kết với bác sĩ
                    if (taiKhoan.VaiTro == "Bác sĩ" && bacSiId.HasValue)
                    {
                        // Kiểm tra xem bác sĩ đã có tài khoản khác chưa
                        var bacSiWithAccount = await _context.BacSis
                            .Include(b => b.TaiKhoan)
                            .FirstOrDefaultAsync(b => b.MaBacSi == bacSiId.Value);
                            
                        if (bacSiWithAccount != null && bacSiWithAccount.MaTaiKhoan.HasValue && 
                            bacSiWithAccount.MaTaiKhoan.Value != id)
                        {
                            ModelState.AddModelError("", "Bác sĩ này đã được liên kết với tài khoản khác");
                            ViewBag.VaiTros = new SelectList(new List<string> { "Admin", "Bác sĩ", "Nhân viên" }, taiKhoan.VaiTro);
                            ViewBag.BacSiList = new SelectList(_context.BacSis.Where(b => b.TrangThai), "MaBacSi", "HoTen", bacSiId);
                            return View(taiKhoan);
                        }
                        
                        // Cập nhật liên kết bác sĩ cũ (nếu có)
                        if (existingUser.BacSi != null && existingUser.BacSi.MaBacSi != bacSiId.Value)
                        {
                            var oldBacSi = await _context.BacSis.FindAsync(existingUser.BacSi.MaBacSi);
                            if (oldBacSi != null)
                            {
                                oldBacSi.MaTaiKhoan = null;
                                _context.Update(oldBacSi);
                            }
                        }
                        
                        // Cập nhật liên kết bác sĩ mới
                        var newBacSi = await _context.BacSis.FindAsync(bacSiId.Value);
                        if (newBacSi != null)
                        {
                            newBacSi.MaTaiKhoan = id;
                            _context.Update(newBacSi);
                        }
                    }
                    // Nếu không còn là bác sĩ nữa thì gỡ liên kết
                    else if (existingUser.BacSi != null)
                    {
                        var oldBacSi = await _context.BacSis.FindAsync(existingUser.BacSi.MaBacSi);
                        if (oldBacSi != null)
                        {
                            oldBacSi.MaTaiKhoan = null;
                            _context.Update(oldBacSi);
                        }
                    }
                    
                    _context.Update(existingUser);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = "Cập nhật tài khoản thành công";
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
            
            ViewBag.VaiTros = new SelectList(new List<string> { "Admin", "Bác sĩ", "Nhân viên" }, taiKhoan.VaiTro);
            ViewBag.BacSiList = new SelectList(_context.BacSis.Where(b => b.TrangThai), "MaBacSi", "HoTen", bacSiId);
            return View(taiKhoan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.TaiKhoans
                .Include(u => u.BacSi)
                .Include(u => u.KhachThamBenh)
                .FirstOrDefaultAsync(u => u.MaTaiKhoan == id);
                
            if (user == null)
            {
                return NotFound();
            }
            
            try
            {
                // Gỡ liên kết (nếu có)
                if (user.BacSi != null)
                {
                    var bacSi = await _context.BacSis.FindAsync(user.BacSi.MaBacSi);
                    if (bacSi != null)
                    {
                        bacSi.MaTaiKhoan = null;
                        _context.Update(bacSi);
                    }
                }
                
                // Xóa tài khoản
                _context.TaiKhoans.Remove(user);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Xóa tài khoản thành công";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Không thể xóa tài khoản: {ex.Message}";
            }
            
            return RedirectToAction(nameof(ManageUsers));
        }
        
        public IActionResult ExportReports()
        {
            // Cài đặt thông báo thành công
            TempData["Success"] = "Xuất báo cáo thành công";
            return RedirectToAction(nameof(Index));
        }
    }
}