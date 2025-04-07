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

            // Lưu ID bệnh nhân vào ViewBag để view có thể xử lý redirect nếu cần
            if (dieuTri.MaBenhNhan.HasValue)
            {
                ViewBag.MaBenhNhan = dieuTri.MaBenhNhan.Value;
            }

            return View(dieuTri);
        }

        public IActionResult Create(int? maBenhNhan)
        {
            ViewBag.MaBenhNhan = new SelectList(_context.BenhNhans.Where(b => b.TrangThai == true), "MaBenhNhan", "HoTen");
            ViewBag.Khoas = new SelectList(_context.Khoas.Where(k => k.TrangThai), "MaKhoa", "TenKhoa");
            
            var model = new DieuTriBenhNhan
            {
                NgayThucHien = DateTime.Now
            };
            
            // Check if maBenhNhan is provided in the URL
            if (maBenhNhan.HasValue)
            {
                var benhNhan = _context.BenhNhans
                    .Include(b => b.Khoa)
                    .Include(b => b.BacSi)
                    .FirstOrDefault(b => b.MaBenhNhan == maBenhNhan.Value);
                    
                if (benhNhan != null)
                {
                    // Set the patient ID in the model
                    model.MaBenhNhan = benhNhan.MaBenhNhan;
                    
                    // Set flag to show the patient is fixed
                    ViewBag.FixedPatient = true;
                    ViewBag.BenhNhanInfo = $"{benhNhan.HoTen} - {benhNhan.Khoa?.TenKhoa ?? "Chưa có khoa"}";
                    
                    // Store the department ID to be pre-selected in the dropdown
                    ViewBag.BenhNhanKhoaId = benhNhan.MaKhoa;
                    
                    // Lock department selection since it's based on patient's current department
                    ViewBag.KhoaIsLocked = true;
                    
                    // Pre-select doctor if assigned to patient
                    if (benhNhan.MaBacSi > 0)
                    {
                        model.MaBacSi = benhNhan.MaBacSi;
                        ViewBag.BacSiIsLocked = true;
                    }
                    
                    // Flag to return to patient details page after saving
                    ViewBag.ReturnToBenhNhanDetail = true;
                    
                    // Get treatment options for the patient's department
                    if (benhNhan.MaKhoa > 0)
                    {
                        var treatments = _context.HinhThucDieuTris
                            .Where(h => h.MaKhoa == benhNhan.MaKhoa)
                            .Select(h => new SelectListItem
                            {
                                Value = h.MaDieuTri.ToString(),
                                Text = $"{h.TenDieuTri} - {h.ChiPhi:N0} VND"
                            })
                            .ToList();
                            
                        ViewBag.MaDieuTri = new SelectList(treatments, "Value", "Text");
                    }
                }
            }
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DieuTriBenhNhan dieuTriBenhNhan, bool returnToBenhNhanDetail = false)
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
                        ViewBag.ReturnToBenhNhanDetail = returnToBenhNhanDetail;
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
                    if (returnToBenhNhanDetail && dieuTriBenhNhan.MaBenhNhan.HasValue)
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
            ViewBag.ReturnToBenhNhanDetail = returnToBenhNhanDetail;
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
            
            // Danh sách khoa cho dropdown
            ViewBag.Khoas = new SelectList(
                await _context.Khoas
                    .Where(k => k.TrangThai)
                    .OrderBy(k => k.TenKhoa)
                    .Select(k => new
                    {
                        MaKhoa = k.MaKhoa,
                        TenKhoa = k.TenKhoa
                    })
                    .ToListAsync(),
                "MaKhoa", "TenKhoa");
            
            // Nếu có mã bệnh nhân, lấy thông tin bệnh nhân và khoa
            if (maBenhNhan.HasValue)
            {
                var benhNhan = await _context.BenhNhans
                    .Include(b => b.Khoa)
                    .FirstOrDefaultAsync(b => b.MaBenhNhan == maBenhNhan.Value);

                if (benhNhan != null)
                {
                    ViewBag.FixedPatient = true;
                    ViewBag.BenhNhanInfo = $"{benhNhan.HoTen} - {benhNhan.MaBenhNhan}";
                    ViewBag.BenhNhanKhoaId = benhNhan.MaKhoa;
                    ViewBag.KhoaIsLocked = true;

                    // Lấy danh sách bác sĩ theo khoa của bệnh nhân
                    var bacSiList = await _context.BacSis
                        .Where(b => b.MaKhoa == benhNhan.MaKhoa)
                        .Select(b => new SelectListItem
                        {
                            Value = b.MaBacSi.ToString(),
                            Text = b.HoTen
                        })
                        .ToListAsync();
                    ViewBag.MaBacSi = new SelectList(bacSiList, "Value", "Text");

                    // Nếu bệnh nhân đã có bác sĩ phụ trách, cố định bác sĩ đó
                    if (benhNhan.MaBacSi > 0)
                    {
                        ViewBag.BacSiIsLocked = true;
                    }

                    // Lấy danh sách hình thức điều trị theo khoa của bệnh nhân
                    var dieuTriList = await _context.HinhThucDieuTris
                        .Where(h => h.MaKhoa == benhNhan.MaKhoa)
                        .Select(h => new SelectListItem
                        {
                            Value = h.MaDieuTri.ToString(),
                            Text = $"{h.TenDieuTri} - {h.ChiPhi:N0} VND"
                        })
                        .ToListAsync();
                    ViewBag.MaDieuTri = new SelectList(dieuTriList, "Value", "Text");
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
                        .Select(h => new
                        {
                            MaDieuTri = h.MaDieuTri,
                            TenDieuTri = $"{h.TenDieuTri} - {h.ChiPhi:N0} VND"
                        })
                        .ToListAsync(), 
                    "MaDieuTri", 
                    "TenDieuTri");
            }
            else
            {
                // Trường hợp không xác định được vai trò/thông tin
                ViewBag.MaBenhNhan = new SelectList(
                    await _context.BenhNhans
                        .Where(b => b.TrangThai)
                        .Select(b => new
                        {
                            MaBenhNhan = b.MaBenhNhan,
                            ThongTinHienThi = $"{b.HoTen} (Mã: {b.MaBenhNhan})"
                        })
                        .ToListAsync(),
                    "MaBenhNhan", "ThongTinHienThi", maBenhNhan);
                    
                ViewBag.MaDieuTri = new SelectList(Enumerable.Empty<SelectListItem>(), "MaDieuTri", "TenDieuTri");
                ViewBag.MaBacSi = new SelectList(await _context.BacSis.Where(b => b.TrangThai).ToListAsync(), "MaBacSi", "HoTen");
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
                    // Lưu thông tin điều trị trước khi cập nhật chi phí
                    var originalDieuTri = await _context.DieuTriBenhNhans
                        .Include(d => d.HinhThucDieuTri)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(d => d.MaDieuTriBenhNhan == id);

                    // Kiểm tra nếu hình thức điều trị đã bị thay đổi, cập nhật chi phí tương ứng
                    if (originalDieuTri != null && originalDieuTri.MaDieuTri != dieuTriBenhNhan.MaDieuTri)
                    {
                        // Lấy thông tin hình thức điều trị cũ và mới
                        var oldHinhThucDieuTri = await _context.HinhThucDieuTris.FindAsync(originalDieuTri.MaDieuTri);
                        var newHinhThucDieuTri = await _context.HinhThucDieuTris.FindAsync(dieuTriBenhNhan.MaDieuTri);

                        if (oldHinhThucDieuTri != null && newHinhThucDieuTri != null)
                        {
                            // Cập nhật chi phí trong bảng ChiPhiDieuTri nếu chưa thanh toán
                            var chiPhi = await _context.ChiPhiDieuTris
                                .FirstOrDefaultAsync(c => c.MaBenhNhan == dieuTriBenhNhan.MaBenhNhan && !c.DaThanhToan);

                            if (chiPhi != null)
                            {
                                // Trừ chi phí cũ và cộng chi phí mới
                                chiPhi.TongChiPhi = chiPhi.TongChiPhi - oldHinhThucDieuTri.ChiPhi + newHinhThucDieuTri.ChiPhi;
                                _context.Update(chiPhi);
                            }
                        }
                    }

                    _context.Update(dieuTriBenhNhan);
                    await _context.SaveChangesAsync();
                    
                    // Thêm thông báo thành công
                    TempData["SuccessMessage"] = "Cập nhật thông tin điều trị thành công!";
                    
                    // Chuyển hướng về trang chi tiết bệnh nhân
                    if (dieuTriBenhNhan.MaBenhNhan.HasValue)
                    {
                        return RedirectToAction("Details", "BenhNhan", new { id = dieuTriBenhNhan.MaBenhNhan.Value });
                    }
                    
                    return RedirectToAction(nameof(Index));
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
            
            if (dieuTri == null)
            {
                return NotFound();
            }
            
            // Lưu MaBenhNhan trước khi xóa để có thể chuyển hướng về trang chi tiết bệnh nhân
            int? maBenhNhan = dieuTri.MaBenhNhan;
            
            try
            {
                // Lấy thông tin về chi phí điều trị để cập nhật
                if (dieuTri.MaDieuTri.HasValue && dieuTri.MaBenhNhan.HasValue)
                {
                    var hinhThucDieuTri = await _context.HinhThucDieuTris.FindAsync(dieuTri.MaDieuTri.Value);
                    
                    if (hinhThucDieuTri != null)
                    {
                        // Cập nhật chi phí trong bảng ChiPhiDieuTri nếu chưa thanh toán
                        var chiPhi = await _context.ChiPhiDieuTris
                            .FirstOrDefaultAsync(c => c.MaBenhNhan == dieuTri.MaBenhNhan.Value && !c.DaThanhToan);

                        if (chiPhi != null)
                        {
                            // Trừ chi phí của hình thức điều trị bị xóa
                            chiPhi.TongChiPhi -= hinhThucDieuTri.ChiPhi;
                            if (chiPhi.TongChiPhi < 0) chiPhi.TongChiPhi = 0; // Đảm bảo chi phí không âm
                            _context.Update(chiPhi);
                        }
                    }
                }
                
                // Xóa điều trị
                _context.DieuTriBenhNhans.Remove(dieuTri);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Xóa thông tin điều trị thành công!";
                
                // Chuyển hướng về trang chi tiết bệnh nhân nếu có
                if (maBenhNhan.HasValue)
                {
                    return RedirectToAction("Details", "BenhNhan", new { id = maBenhNhan.Value });
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi xóa điều trị: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // Phương thức hỗ trợ để lấy danh sách bác sĩ theo khoa
        [HttpGet]
        public async Task<JsonResult> GetBacSiByKhoa(int khoaId)
        {
            try
            {
                var bacSiList = await _context.BacSis
                    .Where(b => b.MaKhoa == khoaId && b.TrangThai)
                    .Select(b => new { 
                        b.MaBacSi, 
                        b.HoTen 
                    })
                    .ToListAsync();
                
                return Json(bacSiList);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
        
        // Phương thức hỗ trợ để lấy danh sách điều trị theo khoa
        [HttpGet]
        public async Task<JsonResult> GetDieuTriByKhoa(int khoaId)
        {
            try
            {
                var dieuTriList = await _context.HinhThucDieuTris
                    .Where(h => h.MaKhoa == khoaId)
                    .Select(h => new { 
                        h.MaDieuTri, 
                        h.TenDieuTri,
                        h.ChiPhi
                    })
                    .ToListAsync();
                
                return Json(dieuTriList);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
    }
}