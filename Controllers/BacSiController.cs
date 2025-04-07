using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyBenhVienNoiTru.Data;
using QuanLyBenhVienNoiTru.Models;
using QuanLyBenhVienNoiTru.Filters;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace QuanLyBenhVienNoiTru.Controllers
{
    [AuthorizeRoles("Admin", "Bác sĩ")]
    public class BacSiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BacSiController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
            {
                // Nếu là Admin, trả về danh sách tất cả bác sĩ
                var bacSis = await _context.BacSis
                    .Include(b => b.Khoa)
                    .OrderBy(b => b.HoTen)
                    .ToListAsync();

                // Lấy số lượng bệnh nhân đang điều trị cho mỗi bác sĩ
                foreach (var bacSi in bacSis)
                {
                    bacSi.BenhNhans = await _context.BenhNhans
                        .Where(b => b.MaBacSi == bacSi.MaBacSi)
                        .ToListAsync();

                    ViewData[$"SoBenhNhanDangDieuTri_{bacSi.MaBacSi}"] = await _context.BenhNhans
                        .Where(b => b.MaBacSi == bacSi.MaBacSi && b.NgayXuatVien == null)
                        .CountAsync();
                }

                ViewBag.MaKhoa = await _context.Khoas
                    .Where(k => k.TrangThai)
                    .Select(k => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Value = k.MaKhoa.ToString(),
                        Text = k.TenKhoa
                    })
                    .ToListAsync();
                    
                return View(bacSis);
            }
            else if (User.IsInRole("Bác sĩ"))
            {
                // Nếu là bác sĩ, trả về các bệnh nhân của họ
                // Đây là một view khác dành cho bác sĩ, không phải danh sách bác sĩ
                var userId = User.FindFirstValue("UserId");
                if (int.TryParse(userId, out int maTaiKhoan))
                {
                    var bacSi = await _context.BacSis
                        .Include(b => b.Khoa)
                        .Include(b => b.BenhNhans)
                        .FirstOrDefaultAsync(b => b.MaTaiKhoan == maTaiKhoan);
                    
                    if (bacSi != null)
                    {
                        return RedirectToAction("DanhSachBenhNhan");
                    }
                }
                
                return NotFound("Không tìm thấy thông tin bác sĩ.");
            }
            
            return NotFound();
        }

        // GET: BacSi/Details/5
        [AuthorizeRoles("Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bacSi = await _context.BacSis
                .Include(b => b.Khoa)
                .Include(b => b.TaiKhoan)
                .FirstOrDefaultAsync(m => m.MaBacSi == id);
                
            if (bacSi == null)
            {
                return NotFound();
            }

            // Load danh sách bệnh nhân đang được bác sĩ này phụ trách
            bacSi.BenhNhans = await _context.BenhNhans
                .Include(b => b.Khoa)
                .Where(b => b.MaBacSi == id)
                .ToListAsync();

            // Lấy thông tin điều trị của bác sĩ
            var dieuTri = await _context.DieuTriBenhNhans
                .Include(d => d.BenhNhan)
                .Include(d => d.HinhThucDieuTri)
                .Where(d => d.MaBacSi == id)
                .OrderByDescending(d => d.NgayThucHien)
                .Take(10)
                .ToListAsync();
                
            ViewBag.DieuTri = dieuTri;
            
            // Lấy số lượng bệnh nhân đang điều trị
            ViewBag.SoBenhNhanDangDieuTri = await _context.BenhNhans
                .Where(b => b.MaBacSi == id && b.NgayXuatVien == null)
                .CountAsync();
                
            // Lấy tổng số bệnh nhân đã điều trị
            ViewBag.TongSoBenhNhan = await _context.BenhNhans
                .Where(b => b.MaBacSi == id)
                .CountAsync();
                
            return View(bacSi);
        }

        // GET: BacSi/Create
        [AuthorizeRoles("Admin")]
        public async Task<IActionResult> Create()
        {
            // Lấy danh sách khoa cho dropdown
            ViewBag.MaKhoa = new SelectList(await _context.Khoas
                .Where(k => k.TrangThai)
                .OrderBy(k => k.TenKhoa)
                .ToListAsync(), "MaKhoa", "TenKhoa");
                
            // Tạo bác sĩ mới với trạng thái mặc định là true (hoạt động)
            var bacSi = new BacSi { TrangThai = true };
            return View(bacSi);
        }

        // POST: BacSi/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRoles("Admin")]
        public async Task<IActionResult> Create([Bind("MaBacSi,HoTen,GioiTinh,NgaySinh,SoDienThoai,Email,DiaChi,MaKhoa,TrangThai")] BacSi bacSi)
        {
            try
            {
                // Kiểm tra thủ công các trường bắt buộc
                if (string.IsNullOrEmpty(bacSi.HoTen))
                {
                    ModelState.AddModelError("HoTen", "Vui lòng nhập họ tên");
                }
                
                if (string.IsNullOrEmpty(bacSi.GioiTinh))
                {
                    ModelState.AddModelError("GioiTinh", "Vui lòng chọn giới tính");
                }
                
                if (bacSi.MaKhoa == 0)
                {
                    ModelState.AddModelError("MaKhoa", "Vui lòng chọn khoa");
                }
                
                if (string.IsNullOrEmpty(bacSi.SoDienThoai))
                {
                    ModelState.AddModelError("SoDienThoai", "Vui lòng nhập số điện thoại");
                }
                else if (!System.Text.RegularExpressions.Regex.IsMatch(bacSi.SoDienThoai, @"^[0-9]{10}$"))
                {
                    ModelState.AddModelError("SoDienThoai", "Số điện thoại phải có 10 chữ số");
                }
                
                if (string.IsNullOrEmpty(bacSi.Email))
                {
                    ModelState.AddModelError("Email", "Vui lòng nhập email");
                }
                else if (!System.Text.RegularExpressions.Regex.IsMatch(bacSi.Email, @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$"))
                {
                    ModelState.AddModelError("Email", "Email không hợp lệ");
                }
                
                // Thiết lập giá trị mặc định cho địa chỉ nếu không có
                if (string.IsNullOrEmpty(bacSi.DiaChi))
                {
                    bacSi.DiaChi = "Chưa cập nhật";
                }
                
                // Thiết lập giá trị mặc định cho ngày vào làm là ngày hiện tại
                bacSi.NgayVaoLam = DateTime.Now;
                
                // Lấy tên khoa để gán cho chuyên khoa
                var khoa = await _context.Khoas.FindAsync(bacSi.MaKhoa);
                if (khoa != null)
                {
                    bacSi.ChuyenKhoa = khoa.TenKhoa;
                }
                
                if (ModelState.IsValid)
                {
                    _context.Add(bacSi);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Thêm bác sĩ {bacSi.HoTen} thành công";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
            }
            
            ViewBag.MaKhoa = new SelectList(await _context.Khoas
                .Where(k => k.TrangThai)
                .OrderBy(k => k.TenKhoa)
                .ToListAsync(), "MaKhoa", "TenKhoa");
            return View(bacSi);
        }

        // GET: BacSi/Edit/5
        [AuthorizeRoles("Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bacSi = await _context.BacSis
                .Include(b => b.TaiKhoan)
                .FirstOrDefaultAsync(b => b.MaBacSi == id);
                
            if (bacSi == null)
            {
                return NotFound();
            }
            
            ViewBag.MaKhoa = new SelectList(await _context.Khoas
                .Where(k => k.TrangThai)
                .OrderBy(k => k.TenKhoa)
                .ToListAsync(), "MaKhoa", "TenKhoa", bacSi.MaKhoa);
                
            return View(bacSi);
        }

        // POST: BacSi/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRoles("Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("MaBacSi,HoTen,GioiTinh,NgaySinh,SoDienThoai,Email,ChuyenKhoa,DiaChi,MaKhoa,MaTaiKhoan,TrangThai")] BacSi bacSi)
        {
            if (id != bacSi.MaBacSi)
            {
                return NotFound();
            }

            try
            {
                // Kiểm tra thủ công các trường bắt buộc
                if (string.IsNullOrEmpty(bacSi.HoTen))
                {
                    ModelState.AddModelError("HoTen", "Vui lòng nhập họ tên");
                }
                
                if (string.IsNullOrEmpty(bacSi.GioiTinh))
                {
                    ModelState.AddModelError("GioiTinh", "Vui lòng chọn giới tính");
                }
                
                if (string.IsNullOrEmpty(bacSi.ChuyenKhoa))
                {
                    ModelState.AddModelError("ChuyenKhoa", "Vui lòng nhập chuyên khoa");
                }
                
                if (bacSi.MaKhoa == 0)
                {
                    ModelState.AddModelError("MaKhoa", "Vui lòng chọn khoa");
                }
                
                if (string.IsNullOrEmpty(bacSi.SoDienThoai))
                {
                    ModelState.AddModelError("SoDienThoai", "Vui lòng nhập số điện thoại");
                }
                else if (!System.Text.RegularExpressions.Regex.IsMatch(bacSi.SoDienThoai, @"^[0-9]{10}$"))
                {
                    ModelState.AddModelError("SoDienThoai", "Số điện thoại phải có 10 chữ số");
                }
                
                if (string.IsNullOrEmpty(bacSi.Email))
                {
                    ModelState.AddModelError("Email", "Vui lòng nhập email");
                }
                else if (!System.Text.RegularExpressions.Regex.IsMatch(bacSi.Email, @"^[a-zA-Z0-9._-]+@([a-zA-Z0-9-]+\.)+[a-zA-Z0-9-]{2,4}$"))
                {
                    ModelState.AddModelError("Email", "Email không hợp lệ");
                }
                
                // Thiết lập giá trị mặc định cho địa chỉ nếu không có
                if (string.IsNullOrEmpty(bacSi.DiaChi))
                {
                    bacSi.DiaChi = "Chưa cập nhật";
                }
                
                if (ModelState.IsValid)
                {
                    try
                    {
                        // Lấy thông tin bác sĩ hiện tại
                        var existingBacSi = await _context.BacSis
                            .AsNoTracking()
                            .FirstOrDefaultAsync(b => b.MaBacSi == id);
                            
                        if (existingBacSi == null)
                        {
                            return NotFound();
                        }
                        
                        // Kiểm tra và cập nhật tài khoản liên kết (nếu có)
                        if (existingBacSi.MaTaiKhoan != bacSi.MaTaiKhoan)
                        {
                            // Nếu có thay đổi về liên kết tài khoản, cập nhật
                            if (bacSi.MaTaiKhoan.HasValue)
                            {
                                // Kiểm tra xem tài khoản có tồn tại và có liên kết với bác sĩ khác không
                                var taiKhoan = await _context.TaiKhoans
                                    .Include(t => t.BacSi)
                                    .FirstOrDefaultAsync(t => t.MaTaiKhoan == bacSi.MaTaiKhoan);
                                    
                                if (taiKhoan != null && taiKhoan.BacSi != null && taiKhoan.BacSi.MaBacSi != id)
                                {
                                    ModelState.AddModelError("MaTaiKhoan", "Tài khoản này đã được liên kết với bác sĩ khác");
                                    ViewBag.MaKhoa = new SelectList(await _context.Khoas
                                        .Where(k => k.TrangThai)
                                        .OrderBy(k => k.TenKhoa)
                                        .ToListAsync(), "MaKhoa", "TenKhoa", bacSi.MaKhoa);
                                    return View(bacSi);
                                }
                            }
                        }
                        
                        _context.Update(bacSi);
                        await _context.SaveChangesAsync();
                        
                        TempData["Success"] = $"Cập nhật thông tin bác sĩ {bacSi.HoTen} thành công";
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!await BacSiExists(bacSi.MaBacSi))
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
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
            }
            
            ViewBag.MaKhoa = new SelectList(await _context.Khoas
                .Where(k => k.TrangThai)
                .OrderBy(k => k.TenKhoa)
                .ToListAsync(), "MaKhoa", "TenKhoa", bacSi.MaKhoa);
                
            return View(bacSi);
        }

        // GET: BacSi/Delete/5
        [AuthorizeRoles("Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bacSi = await _context.BacSis
                .Include(b => b.Khoa)
                .Include(b => b.BenhNhans)
                .Include(b => b.TaiKhoan)
                .FirstOrDefaultAsync(m => m.MaBacSi == id);
                
            if (bacSi == null)
            {
                return NotFound();
            }

            // Kiểm tra xem bác sĩ có bệnh nhân đang điều trị không
            var hasBenhNhan = await _context.BenhNhans
                .AnyAsync(b => b.MaBacSi == id && b.NgayXuatVien == null);
                
            ViewBag.CoThongTinLienQuan = hasBenhNhan;
            
            if (hasBenhNhan)
            {
                ViewBag.ThongBao = "Bác sĩ này đang có bệnh nhân điều trị. Nếu xóa, các bệnh nhân sẽ không có bác sĩ phụ trách.";
            }

            return View(bacSi);
        }

        // POST: BacSi/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeRoles("Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var bacSi = await _context.BacSis
                    .Include(b => b.TaiKhoan)
                    .FirstOrDefaultAsync(b => b.MaBacSi == id);
                    
                if (bacSi == null)
                {
                    return NotFound();
                }
                
                // Kiểm tra xem bác sĩ có bệnh nhân đang điều trị hay không
                var hasBenhNhan = await _context.BenhNhans
                    .AnyAsync(b => b.MaBacSi == id && b.NgayXuatVien == null);
                    
                if (hasBenhNhan)
                {
                    // Cập nhật để các bệnh nhân không còn bác sĩ phụ trách
                    var benhNhans = await _context.BenhNhans
                        .Where(b => b.MaBacSi == id)
                        .ToListAsync();
                        
                    foreach (var bn in benhNhans)
                    {
                        bn.MaBacSi = null;
                        _context.Update(bn);
                    }
                }
                
                // Gỡ bỏ liên kết với tài khoản nếu có
                if (bacSi.MaTaiKhoan.HasValue)
                {
                    var taiKhoan = await _context.TaiKhoans.FindAsync(bacSi.MaTaiKhoan.Value);
                    if (taiKhoan != null)
                    {
                        // Không xóa tài khoản, chỉ gỡ liên kết
                        taiKhoan.BacSi = null;
                        _context.Update(taiKhoan);
                    }
                }
                
                // Đánh dấu trạng thái là không hoạt động thay vì xóa
                bacSi.TrangThai = false;
                _context.Update(bacSi);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = $"Đã xóa bác sĩ {bacSi.HoTen} thành công";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Không thể xóa bác sĩ: {ex.Message}";
            }
            
            return RedirectToAction(nameof(Index));
        }

        // Action mới để hiển thị danh sách bệnh nhân cho bác sĩ
        public async Task<IActionResult> DanhSachBenhNhan()
        {
            var userId = User.FindFirstValue("UserId");
            var bacSiId = 0;
            
            if (User.IsInRole("Bác sĩ") && int.TryParse(userId, out int maTaiKhoan))
            {
                var bacSi = await _context.BacSis.FirstOrDefaultAsync(b => b.MaTaiKhoan == maTaiKhoan);
                if (bacSi != null)
                {
                    bacSiId = bacSi.MaBacSi;
                }
            }
            
            // Lấy danh sách tất cả bệnh nhân đang điều trị (chưa xuất viện)
            var benhNhanList = await _context.BenhNhans
                .Where(b => b.NgayXuatVien == null)
                .Include(b => b.Khoa)
                .Include(b => b.BacSi) // Thêm Include cho bác sĩ chính
                .Include(b => b.DieuTriBenhNhans)
                    .ThenInclude(d => d.BacSi)
                .OrderByDescending(b => b.NgayNhapVien) // Sắp xếp theo ngày nhập viện giảm dần
                .ToListAsync();
                
            if (User.IsInRole("Bác sĩ") && bacSiId > 0)
            {
                // Nếu là bác sĩ, chỉ hiển thị bệnh nhân của bác sĩ đó hoặc chưa có bác sĩ nào
                benhNhanList = benhNhanList
                    .Where(b => b.MaBacSi == bacSiId || 
                                b.DieuTriBenhNhans.Any(d => d.MaBacSi == bacSiId) || 
                                (b.MaBacSi == null && !b.DieuTriBenhNhans.Any()))
                    .ToList();
            }
            
            // Lấy danh sách khoa và bác sĩ cho filter
            var danhSachKhoa = await _context.Khoas
                .Where(k => k.TrangThai)
                .Select(k => k.TenKhoa)
                .ToListAsync();
                
            var danhSachBacSi = await _context.BacSis
                .Where(b => b.TrangThai)
                .Select(b => b.HoTen)
                .ToListAsync();
                
            ViewBag.DanhSachKhoa = danhSachKhoa;
            ViewBag.DanhSachBacSi = danhSachBacSi;
            
            // Đếm số lượng bệnh nhân
            ViewBag.TongSoBenhNhan = benhNhanList.Count;
            ViewBag.SoBenhNhanChuaPhucVu = benhNhanList.Count(b => b.MaBacSi == null);
            
            return View(benhNhanList);
        }
        
        public async Task<IActionResult> ThemDieuTri(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            // Thay vì hiển thị form tại đây, chuyển hướng đến DieuTriBenhNhan/Create
            return RedirectToAction("Create", "DieuTriBenhNhan", new { maBenhNhan = id });
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemDieuTri(DieuTriBenhNhan dieuTriBenhNhan)
        {
            if (ModelState.IsValid)
            {
                _context.Add(dieuTriBenhNhan);
                await _context.SaveChangesAsync();
                
                // Cập nhật chi phí
                var hinhThucDieuTri = await _context.HinhThucDieuTris
                    .FindAsync(dieuTriBenhNhan.MaDieuTri);
                    
                var chiPhiHienTai = await _context.ChiPhiDieuTris
                    .FirstOrDefaultAsync(c => c.MaBenhNhan == dieuTriBenhNhan.MaBenhNhan && !c.DaThanhToan);
                    
                if (chiPhiHienTai == null)
                {
                    chiPhiHienTai = new ChiPhiDieuTri
                    {
                        MaBenhNhan = dieuTriBenhNhan.MaBenhNhan.Value,
                        TongChiPhi = hinhThucDieuTri.ChiPhi,
                        DaThanhToan = false,
                        NgayLap = DateTime.Now
                    };
                    _context.Add(chiPhiHienTai);
                }
                else
                {
                    chiPhiHienTai.TongChiPhi += hinhThucDieuTri.ChiPhi;
                }
                
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.MaDieuTri = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _context.HinhThucDieuTris.ToListAsync(), "MaDieuTri", "TenDieuTri");
                
            ViewBag.MaBacSi = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _context.BacSis.ToListAsync(), "MaBacSi", "HoTen");
                
            return View(dieuTriBenhNhan);
        }
        
        public async Task<IActionResult> XuatVien(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var benhNhan = await _context.BenhNhans
                .Include(b => b.ChiPhiDieuTris)
                .FirstOrDefaultAsync(b => b.MaBenhNhan == id);
                
            if (benhNhan == null)
            {
                return NotFound();
            }
            
            if (benhNhan.NgayXuatVien != null)
            {
                return RedirectToAction(nameof(Index));
            }
            
            var chiPhi = benhNhan.ChiPhiDieuTris.FirstOrDefault(c => !c.DaThanhToan);
            
            if (chiPhi != null && !chiPhi.DaThanhToan)
            {
                TempData["Error"] = "Bệnh nhân còn chi phí chưa thanh toán. Vui lòng thanh toán trước khi xuất viện.";
                return RedirectToAction("Details", "BenhNhan", new { id = benhNhan.MaBenhNhan });
            }
            
            return View(benhNhan);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XuatVienConfirmed(int id)
        {
            var benhNhan = await _context.BenhNhans.FindAsync(id);
            
            if (benhNhan == null)
            {
                return NotFound();
            }
            
            benhNhan.NgayXuatVien = DateTime.Now;
            benhNhan.TrangThai = false; // Cập nhật trạng thái thành đã xuất viện
            _context.Update(benhNhan);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = $"Bệnh nhân {benhNhan.HoTen} đã được xuất viện thành công.";
            return RedirectToAction("DanhSachBenhNhan");
        }

        private async Task<bool> BacSiExists(int id)
        {
            return await _context.BacSis.AnyAsync(e => e.MaBacSi == id);
        }
    }
}