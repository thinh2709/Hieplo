using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyBenhVienNoiTru.Data;
using QuanLyBenhVienNoiTru.Models;
using QuanLyBenhVienNoiTru.Filters;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyBenhVienNoiTru.Controllers
{
    [AuthorizeRoles("Admin", "Bác sĩ")]
    public class DieuTriBenhNhanController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DieuTriBenhNhanController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var dieuTriList = await _context.DieuTriBenhNhans
                .Include(d => d.BenhNhan)
                .Include(d => d.HinhThucDieuTri)
                .Include(d => d.BacSi)
                .ToListAsync();
                
            return View(dieuTriList);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dieuTri = await _context.DieuTriBenhNhans
                .Include(d => d.BenhNhan)
                .Include(d => d.HinhThucDieuTri)
                .Include(d => d.BacSi)
                .FirstOrDefaultAsync(d => d.MaDieuTriBenhNhan == id);
                
            if (dieuTri == null)
            {
                return NotFound();
            }

            return View(dieuTri);
        }

        public async Task<IActionResult> Create(int? maBenhNhan = null)
        {
            // Chuẩn bị dữ liệu cho view
            await PrepareCreateViewData(maBenhNhan);
            
            // Nếu có mã bệnh nhân được truyền vào, tạo model với thông tin mặc định
            if (maBenhNhan.HasValue)
            {
                var benhNhan = await _context.BenhNhans
                    .Include(b => b.Khoa)
                    .FirstOrDefaultAsync(b => b.MaBenhNhan == maBenhNhan.Value);
                    
                if (benhNhan != null)
                {
                    // Thêm thông tin để View biết rằng bệnh nhân đã được chọn sẵn
                    ViewBag.FixedPatient = true;
                    ViewBag.BenhNhanInfo = $"{benhNhan.HoTen} (Mã: {benhNhan.MaBenhNhan} - Khoa: {benhNhan.Khoa.TenKhoa})";
                    
                    var model = new DieuTriBenhNhan
                    {
                        MaBenhNhan = maBenhNhan.Value,
                        NgayThucHien = DateTime.Now
                    };
                    
                    // Tự động chọn bác sĩ nếu người dùng là bác sĩ
                    var currentUser = await _context.TaiKhoans
                        .Include(t => t.BacSi)
                        .FirstOrDefaultAsync(t => t.TenDangNhap == User.Identity.Name);
                        
                    if (User.IsInRole("Bác sĩ") && currentUser?.BacSi != null)
                    {
                        model.MaBacSi = currentUser.BacSi.MaBacSi;
                    }
                    
                    return View(model);
                }
            }
            
            // Mặc định trả về view trống
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DieuTriBenhNhan dieuTriBenhNhan)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Đảm bảo các giá trị đầu vào không null trước khi sử dụng
                    if (!dieuTriBenhNhan.MaBenhNhan.HasValue || !dieuTriBenhNhan.MaDieuTri.HasValue)
                    {
                        ModelState.AddModelError("", "Vui lòng chọn bệnh nhân và hình thức điều trị");
                        
                        // Chuẩn bị lại dữ liệu cho view
                        await PrepareCreateViewData(dieuTriBenhNhan.MaBenhNhan);
                        return View(dieuTriBenhNhan);
                    }
                    
                    // Đảm bảo ngày thực hiện được thiết lập
                    if (!dieuTriBenhNhan.NgayThucHien.HasValue)
                    {
                        dieuTriBenhNhan.NgayThucHien = DateTime.Now;
                    }
                    
                    _context.Add(dieuTriBenhNhan);
                    await _context.SaveChangesAsync();
                    
                    // Cập nhật chi phí
                    var hinhThucDieuTri = await _context.HinhThucDieuTris
                        .FindAsync(dieuTriBenhNhan.MaDieuTri.Value);
                        
                    if (hinhThucDieuTri != null)
                    {
                        var chiPhiHienTai = await _context.ChiPhiDieuTris
                            .FirstOrDefaultAsync(c => c.MaBenhNhan == dieuTriBenhNhan.MaBenhNhan.Value && !c.DaThanhToan);
                            
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
                    }
                    
                    TempData["SuccessMessage"] = "Đã thêm điều trị mới thành công!";
                    
                    // Chuyển hướng về trang chi tiết bệnh nhân nếu được tạo từ đó
                    if (dieuTriBenhNhan.MaBenhNhan.HasValue)
                    {
                        return RedirectToAction("Details", "BenhNhan", new { id = dieuTriBenhNhan.MaBenhNhan.Value });
                    }
                    
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Đã xảy ra lỗi: {ex.Message}");
                }
            }
            
            // Nếu có lỗi, chuẩn bị lại dữ liệu cho view
            await PrepareCreateViewData(dieuTriBenhNhan.MaBenhNhan);
            return View(dieuTriBenhNhan);
        }
        
        // Phương thức giúp chuẩn bị dữ liệu cho view Create
        private async Task PrepareCreateViewData(int? maBenhNhan)
        {
            // Lấy thông tin người dùng hiện tại
            var currentUser = await _context.TaiKhoans
                .Include(t => t.BacSi)
                .FirstOrDefaultAsync(t => t.TenDangNhap == User.Identity.Name);

            // Kiểm tra vai trò người dùng
            bool isAdmin = User.IsInRole("Admin");
            bool isDoctor = User.IsInRole("Bác sĩ");
            
            // Chuẩn bị dữ liệu cho view
            if (isAdmin)
            {
                ViewBag.MaBenhNhan = new SelectList(
                    await _context.BenhNhans
                        .Where(b => b.TrangThai)
                        .Select(b => new
                        {
                            MaBenhNhan = b.MaBenhNhan,
                            ThongTinHienThi = $"{b.HoTen} (Mã: {b.MaBenhNhan} - Khoa: {b.Khoa.TenKhoa})"
                        })
                        .ToListAsync(),
                    "MaBenhNhan", "ThongTinHienThi", maBenhNhan);
                
                // Danh sách bác sĩ
                ViewBag.MaBacSi = new SelectList(await _context.BacSis.ToListAsync(), "MaBacSi", "HoTen");
                
                // Ban đầu, hiển thị tất cả hình thức điều trị
                ViewBag.MaDieuTri = new SelectList(await _context.HinhThucDieuTris.ToListAsync(), "MaDieuTri", "TenDieuTri");
                
                // Thêm danh sách khoa cho dropdown lọc
                ViewBag.Khoas = new SelectList(await _context.Khoas.ToListAsync(), "MaKhoa", "TenKhoa");
                
                // Nếu có mã bệnh nhân, lấy thông tin khoa của bệnh nhân đó
                if (maBenhNhan.HasValue)
                {
                    var benhNhan = await _context.BenhNhans.FindAsync(maBenhNhan.Value);
                    if (benhNhan != null)
                    {
                        ViewBag.SelectedKhoa = benhNhan.MaKhoa;
                    }
                }
            }
            else if (isDoctor && currentUser?.BacSi != null)
            {
                // Nếu là Bác sĩ: chỉ hiển thị bệnh nhân của khoa bác sĩ đang quản lý và tự động chọn bác sĩ đó
                var bacSi = currentUser.BacSi;
                int maKhoa = bacSi.MaKhoa;

                // Danh sách bệnh nhân thuộc khoa của bác sĩ
                ViewBag.MaBenhNhan = new SelectList(
                    await _context.BenhNhans
                        .Where(b => b.TrangThai && b.MaKhoa == maKhoa)
                        .Select(b => new
                        {
                            MaBenhNhan = b.MaBenhNhan,
                            ThongTinHienThi = $"{b.HoTen} (Mã: {b.MaBenhNhan} - Khoa: {b.Khoa.TenKhoa})"
                        })
                        .ToListAsync(),
                    "MaBenhNhan", "ThongTinHienThi", maBenhNhan);
                
                // Tự động chọn bác sĩ hiện tại
                ViewBag.MaBacSi = new SelectList(
                    new[] { bacSi }, 
                    "MaBacSi", 
                    "HoTen", 
                    bacSi.MaBacSi);
                
                // Chỉ hiển thị phương pháp điều trị thuộc khoa của bác sĩ
                ViewBag.MaDieuTri = new SelectList(
                    await _context.HinhThucDieuTris
                        .Where(h => h.MaKhoa == maKhoa)
                        .ToListAsync(), 
                    "MaDieuTri", 
                    "TenDieuTri");
            }
            else
            {
                // Trường hợp không xác định được vai trò/thông tin
                ViewBag.MaBenhNhan = new SelectList(await _context.BenhNhans.Where(b => b.TrangThai).ToListAsync(), "MaBenhNhan", "HoTen", maBenhNhan);
                ViewBag.MaDieuTri = new SelectList(await _context.HinhThucDieuTris.ToListAsync(), "MaDieuTri", "TenDieuTri");
                ViewBag.MaBacSi = new SelectList(await _context.BacSis.ToListAsync(), "MaBacSi", "HoTen");
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dieuTri = await _context.DieuTriBenhNhans.FindAsync(id);
            if (dieuTri == null)
            {
                return NotFound();
            }
            
            ViewBag.MaBenhNhan = new SelectList(await _context.BenhNhans.ToListAsync(), "MaBenhNhan", "HoTen", dieuTri.MaBenhNhan);
            ViewBag.MaDieuTri = new SelectList(await _context.HinhThucDieuTris.ToListAsync(), "MaDieuTri", "TenDieuTri", dieuTri.MaDieuTri);
            ViewBag.MaBacSi = new SelectList(await _context.BacSis.ToListAsync(), "MaBacSi", "HoTen", dieuTri.MaBacSi);
            
            return View(dieuTri);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DieuTriBenhNhan dieuTriBenhNhan)
        {
            if (id != dieuTriBenhNhan.MaDieuTriBenhNhan)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dieuTriBenhNhan);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.DieuTriBenhNhans.AnyAsync(e => e.MaDieuTriBenhNhan == id))
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
            
            ViewBag.MaBenhNhan = new SelectList(await _context.BenhNhans.ToListAsync(), "MaBenhNhan", "HoTen", dieuTriBenhNhan.MaBenhNhan);
            ViewBag.MaDieuTri = new SelectList(await _context.HinhThucDieuTris.ToListAsync(), "MaDieuTri", "TenDieuTri", dieuTriBenhNhan.MaDieuTri);
            ViewBag.MaBacSi = new SelectList(await _context.BacSis.ToListAsync(), "MaBacSi", "HoTen", dieuTriBenhNhan.MaBacSi);
            
            return View(dieuTriBenhNhan);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dieuTri = await _context.DieuTriBenhNhans
                .Include(d => d.BenhNhan)
                .Include(d => d.HinhThucDieuTri)
                .Include(d => d.BacSi)
                .FirstOrDefaultAsync(d => d.MaDieuTriBenhNhan == id);
                
            if (dieuTri == null)
            {
                return NotFound();
            }

            return View(dieuTri);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dieuTri = await _context.DieuTriBenhNhans.FindAsync(id);
            _context.DieuTriBenhNhans.Remove(dieuTri);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}