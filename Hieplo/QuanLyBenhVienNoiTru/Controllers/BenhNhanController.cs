using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyBenhVienNoiTru.Data;
using QuanLyBenhVienNoiTru.Filters;
using QuanLyBenhVienNoiTru.Models;
using QuanLyBenhVienNoiTru.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;

namespace QuanLyBenhVienNoiTru.Controllers
{
    [AuthorizeRoles("Admin", "Bác sĩ")]
    public class BenhNhanController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BenhNhanController> _logger;

        public BenhNhanController(ApplicationDbContext context, ILogger<BenhNhanController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var benhNhans = await _context.BenhNhans
                .Include(b => b.Khoa)
                .ToListAsync();
                
            return View(benhNhans);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var benhNhan = await _context.BenhNhans
                .Include(b => b.Khoa)
                .Include(b => b.BacSi)
                .Include(b => b.DieuTriBenhNhans)
                    .ThenInclude(d => d.HinhThucDieuTri)
                .Include(b => b.DieuTriBenhNhans)
                    .ThenInclude(d => d.BacSi)
                .Include(b => b.ChiPhiDieuTris)
                .Include(b => b.LichThamBenhs)
                    .ThenInclude(l => l.KhachThamBenh)
                .FirstOrDefaultAsync(m => m.MaBenhNhan == id);
                
            if (benhNhan == null)
            {
                return NotFound();
            }

            // Tạo view model cho chi tiết bệnh nhân
            var viewModel = new BenhNhanDetailsViewModel
            {
                BenhNhan = benhNhan,
                DieuTris = benhNhan.DieuTriBenhNhans?.ToList() ?? new List<DieuTriBenhNhan>(),
                ChiPhis = benhNhan.ChiPhiDieuTris?.ToList() ?? new List<ChiPhiDieuTri>(),
                LichThamBenhs = benhNhan.LichThamBenhs?.ToList() ?? new List<LichThamBenh>(),
                TongChiPhiChuaThanhToan = benhNhan.ChiPhiDieuTris?.Where(c => !c.DaThanhToan).Sum(c => c.TongChiPhi) ?? 0,
                TongChiPhiDaThanhToan = benhNhan.ChiPhiDieuTris?.Where(c => c.DaThanhToan).Sum(c => c.TongChiPhi) ?? 0
            };

            return View(viewModel);
        }

        public IActionResult Create()
        {
            var viewModel = new BenhNhanCreateWithTreatmentViewModel
            {
                BenhNhan = new BenhNhan 
                { 
                    NgayNhapVien = DateTime.Now, 
                    TrangThai = true
                },
                KhoaSelectList = new SelectList(_context.Khoas, "MaKhoa", "TenKhoa"),
                GioiTinhOptions = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Nam", Text = "Nam" },
                    new SelectListItem { Value = "Nữ", Text = "Nữ" }
                },
                HinhThucDieuTriSelectList = new SelectList(_context.HinhThucDieuTris, "MaDieuTri", "TenDieuTri"),
                BacSiSelectList = new SelectList(_context.BacSis, "MaBacSi", "HoTen")
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BenhNhanCreateWithTreatmentViewModel viewModel)
        {
            try
            {
                // Loại bỏ validation errors cho các navigation properties
                foreach (var key in ModelState.Keys.Where(k => k.StartsWith("BenhNhan.Khoa") || 
                                                           k.StartsWith("BenhNhan.BacSi") || 
                                                           k.StartsWith("BenhNhan.DieuTriBenhNhans") || 
                                                           k.StartsWith("BenhNhan.LichThamBenhs") || 
                                                           k.StartsWith("BenhNhan.ChiPhiDieuTris") || 
                                                           k.StartsWith("BenhNhan.HinhThucDieuTris")).ToList())
                {
                    ModelState.Remove(key);
                }

                if (!ModelState.IsValid)
                {
                    // Log validation errors
                    string validationErrors = string.Join("; ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));

                    _logger.LogError($"Lỗi validation khi thêm bệnh nhân mới: {validationErrors}");
                    
                    // Prepare detailed validation errors for the view
                    var errorList = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .Select(x => new { 
                            Property = x.Key, 
                            Errors = x.Value.Errors.Select(e => e.ErrorMessage).ToList() 
                        })
                        .ToList();
                    
                    ViewBag.ValidationErrors = errorList;
                    
                    // Khởi tạo lại các SelectList
                    viewModel.KhoaSelectList = new SelectList(_context.Khoas, "MaKhoa", "TenKhoa", viewModel.BenhNhan?.MaKhoa);
                    viewModel.HinhThucDieuTriSelectList = new SelectList(_context.HinhThucDieuTris, "MaDieuTri", "TenDieuTri");
                    viewModel.BacSiSelectList = new SelectList(_context.BacSis, "MaBacSi", "HoTen", viewModel.MaBacSi);
                    
                    if (viewModel.GioiTinhOptions == null)
                    {
                        viewModel.GioiTinhOptions = new List<SelectListItem>
                        {
                            new SelectListItem { Value = "Nam", Text = "Nam" },
                            new SelectListItem { Value = "Nữ", Text = "Nữ" }
                        };
                    }
                    
                    // Khởi tạo các collection cần thiết
                    if (viewModel.BenhNhan != null)
                    {
                        viewModel.BenhNhan.DieuTriBenhNhans = new List<DieuTriBenhNhan>();
                        viewModel.BenhNhan.LichThamBenhs = new List<LichThamBenh>();
                        viewModel.BenhNhan.ChiPhiDieuTris = new List<ChiPhiDieuTri>();
                        viewModel.BenhNhan.HinhThucDieuTris = new List<HinhThucDieuTri>();
                    }
                    
                    return View(viewModel);
                }

                // Đảm bảo ngày nhập viện được thiết lập
                if (viewModel.BenhNhan.NgayNhapVien == default)
                {
                    viewModel.BenhNhan.NgayNhapVien = DateTime.Now;
                }
                
                // Đảm bảo trạng thái bệnh nhân được thiết lập đúng (đang điều trị)
                viewModel.BenhNhan.TrangThai = true;
                
                // Khởi tạo các collection để tránh lỗi khi lưu
                viewModel.BenhNhan.DieuTriBenhNhans = new List<DieuTriBenhNhan>();
                viewModel.BenhNhan.LichThamBenhs = new List<LichThamBenh>();
                viewModel.BenhNhan.ChiPhiDieuTris = new List<ChiPhiDieuTri>();
                viewModel.BenhNhan.HinhThucDieuTris = new List<HinhThucDieuTri>();

                // Log patient information before saving
                _logger.LogInformation($"Thêm bệnh nhân mới: Tên={viewModel.BenhNhan.HoTen}, Giới tính={viewModel.BenhNhan.GioiTinh}, MaKhoa={viewModel.BenhNhan.MaKhoa}");

                // Tạo một bệnh nhân mới từ dữ liệu đã nhập để tránh lỗi với các navigation properties
                var newBenhNhan = new BenhNhan
                {
                    HoTen = viewModel.BenhNhan.HoTen,
                    NgaySinh = viewModel.BenhNhan.NgaySinh,
                    GioiTinh = viewModel.BenhNhan.GioiTinh,
                    SoDienThoai = viewModel.BenhNhan.SoDienThoai,
                    Email = viewModel.BenhNhan.Email,
                    DiaChi = viewModel.BenhNhan.DiaChi,
                    BaoHiemYTe = viewModel.BenhNhan.BaoHiemYTe,
                    NgayNhapVien = viewModel.BenhNhan.NgayNhapVien,
                    NgayXuatVien = viewModel.BenhNhan.NgayXuatVien,
                    MaKhoa = viewModel.BenhNhan.MaKhoa,
                    ChanDoan = viewModel.BenhNhan.ChanDoan,
                    TrangThai = viewModel.BenhNhan.TrangThai
                };

                // Thêm bệnh nhân vào database
                _context.BenhNhans.Add(newBenhNhan);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Đã thêm bệnh nhân thành công: {newBenhNhan.MaBenhNhan}");
                
                // Nếu người dùng chọn thêm hình thức điều trị
                if (viewModel.ThemDieuTri && viewModel.HinhThucDieuTriIds?.Count > 0)
                {
                    // Thêm các hình thức điều trị được chọn
                    foreach (var dieuTriId in viewModel.HinhThucDieuTriIds)
                    {
                        var dieuTri = new DieuTriBenhNhan
                        {
                            MaBenhNhan = newBenhNhan.MaBenhNhan,
                            MaDieuTri = dieuTriId,
                            MaBacSi = viewModel.MaBacSi,
                            NgayThucHien = viewModel.NgayThucHien ?? DateTime.Now
                        };
                        
                        _context.DieuTriBenhNhans.Add(dieuTri);
                        
                        // Cập nhật chi phí
                        var hinhThucDieuTri = await _context.HinhThucDieuTris.FindAsync(dieuTriId);
                        
                        if (hinhThucDieuTri != null)
                        {
                            var chiPhiHienTai = await _context.ChiPhiDieuTris
                                .FirstOrDefaultAsync(c => c.MaBenhNhan == newBenhNhan.MaBenhNhan && !c.DaThanhToan);
                                
                            if (chiPhiHienTai == null)
                            {
                                chiPhiHienTai = new ChiPhiDieuTri
                                {
                                    MaBenhNhan = newBenhNhan.MaBenhNhan,
                                    TongChiPhi = hinhThucDieuTri.ChiPhi,
                                    DaThanhToan = false,
                                    NgayLap = DateTime.Now
                                };
                                _context.ChiPhiDieuTris.Add(chiPhiHienTai);
                            }
                            else
                            {
                                chiPhiHienTai.TongChiPhi += hinhThucDieuTri.ChiPhi;
                            }
                        }
                    }
                    
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Đã thêm {viewModel.HinhThucDieuTriIds.Count} hình thức điều trị cho bệnh nhân {newBenhNhan.MaBenhNhan}");
                }
                
                TempData["SuccessMessage"] = "Thêm bệnh nhân mới thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log exception details
                _logger.LogError(ex, "Lỗi khi thêm bệnh nhân mới");
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner exception: {ex.InnerException.Message}");
                }

                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi thêm bệnh nhân. Vui lòng thử lại.";
                
                // Khởi tạo lại các SelectList
                viewModel.KhoaSelectList = new SelectList(_context.Khoas, "MaKhoa", "TenKhoa", viewModel.BenhNhan?.MaKhoa);
                viewModel.HinhThucDieuTriSelectList = new SelectList(_context.HinhThucDieuTris, "MaDieuTri", "TenDieuTri");
                viewModel.BacSiSelectList = new SelectList(_context.BacSis, "MaBacSi", "HoTen", viewModel.MaBacSi);
                
                if (viewModel.GioiTinhOptions == null)
                {
                    viewModel.GioiTinhOptions = new List<SelectListItem>
                    {
                        new SelectListItem { Value = "Nam", Text = "Nam" },
                        new SelectListItem { Value = "Nữ", Text = "Nữ" }
                    };
                }
                
                return View(viewModel);
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var benhNhan = await _context.BenhNhans
                .Include(b => b.Khoa)
                .Include(b => b.BacSi)
                .Include(b => b.DieuTriBenhNhans)
                .FirstOrDefaultAsync(m => m.MaBenhNhan == id);
                
            if (benhNhan == null)
            {
                return NotFound();
            }

            // Tạo view model cho chỉnh sửa bệnh nhân
            var viewModel = new BenhNhanEditViewModel
            {
                MaBenhNhan = benhNhan.MaBenhNhan,
                HoTen = benhNhan.HoTen,
                NgaySinh = benhNhan.NgaySinh,
                GioiTinh = benhNhan.GioiTinh,
                SoDienThoai = benhNhan.SoDienThoai,
                Email = benhNhan.Email,
                DiaChi = benhNhan.DiaChi,
                BaoHiemYTe = benhNhan.BaoHiemYTe,
                NgayNhapVien = benhNhan.NgayNhapVien,
                NgayXuatVien = benhNhan.NgayXuatVien,
                MaKhoa = benhNhan.MaKhoa,
                ChanDoan = benhNhan.ChanDoan,
                TrangThai = benhNhan.TrangThai,
                
                // Thêm thông tin điều trị 
                ThemDieuTri = false,
                MaBacSi = benhNhan.MaBacSi,
                HinhThucDieuTriIds = new List<int>(),
                
                // Danh sách lựa chọn
                KhoaSelectList = new SelectList(_context.Khoas, "MaKhoa", "TenKhoa", benhNhan.MaKhoa),
                GioiTinhOptions = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Nam", Text = "Nam", Selected = benhNhan.GioiTinh == "Nam" },
                    new SelectListItem { Value = "Nữ", Text = "Nữ", Selected = benhNhan.GioiTinh == "Nữ" }
                },
                BacSiSelectList = new SelectList(_context.BacSis.Where(b => b.MaKhoa == benhNhan.MaKhoa), "MaBacSi", "HoTen", benhNhan.MaBacSi),
                HinhThucDieuTriSelectList = new SelectList(_context.HinhThucDieuTris.Where(h => h.MaKhoa == benhNhan.MaKhoa), "MaDieuTri", "TenDieuTri")
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BenhNhanEditViewModel viewModel)
        {
            if (id != viewModel.MaBenhNhan)
            {
                return NotFound();
            }

            // Loại bỏ validation errors cho các navigation properties
            foreach (var key in ModelState.Keys.Where(k => k.Contains("BacSi.") || 
                                                      k.Contains("Khoa.") || 
                                                      k.Contains("DieuTriBenhNhans") || 
                                                      k.Contains("LichThamBenhs") || 
                                                      k.Contains("ChiPhiDieuTris") || 
                                                      k.Contains("HinhThucDieuTris")).ToList())
            {
                ModelState.Remove(key);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy bệnh nhân hiện tại từ database
                    var benhNhan = await _context.BenhNhans.FindAsync(id);
                    if (benhNhan == null)
                    {
                        return NotFound();
                    }

                    // Cập nhật thông tin
                    benhNhan.HoTen = viewModel.HoTen;
                    benhNhan.NgaySinh = viewModel.NgaySinh;
                    benhNhan.GioiTinh = viewModel.GioiTinh;
                    benhNhan.SoDienThoai = viewModel.SoDienThoai;
                    benhNhan.Email = viewModel.Email;
                    benhNhan.DiaChi = viewModel.DiaChi;
                    benhNhan.BaoHiemYTe = viewModel.BaoHiemYTe;
                    benhNhan.NgayNhapVien = viewModel.NgayNhapVien;
                    benhNhan.NgayXuatVien = viewModel.NgayXuatVien;
                    benhNhan.MaKhoa = viewModel.MaKhoa;
                    benhNhan.ChanDoan = viewModel.ChanDoan;
                    benhNhan.TrangThai = viewModel.TrangThai;
                    
                    // Nếu có chọn bác sĩ
                    if (viewModel.MaBacSi.HasValue)
                    {
                        benhNhan.MaBacSi = viewModel.MaBacSi;
                    }

                    // Cập nhật bệnh nhân
                    _context.Update(benhNhan);
                    await _context.SaveChangesAsync();
                    
                    // Nếu người dùng chọn thêm hình thức điều trị
                    if (viewModel.ThemDieuTri && viewModel.HinhThucDieuTriIds?.Count > 0)
                    {
                        // Thêm các hình thức điều trị được chọn
                        foreach (var dieuTriId in viewModel.HinhThucDieuTriIds)
                        {
                            var dieuTri = new DieuTriBenhNhan
                            {
                                MaBenhNhan = benhNhan.MaBenhNhan,
                                MaDieuTri = dieuTriId,
                                MaBacSi = viewModel.DieuTriMaBacSi ?? viewModel.MaBacSi,
                                NgayThucHien = viewModel.NgayThucHien ?? DateTime.Now
                            };
                            
                            _context.DieuTriBenhNhans.Add(dieuTri);
                            
                            // Cập nhật chi phí
                            var hinhThucDieuTri = await _context.HinhThucDieuTris.FindAsync(dieuTriId);
                            
                            if (hinhThucDieuTri != null)
                            {
                                var chiPhiHienTai = await _context.ChiPhiDieuTris
                                    .FirstOrDefaultAsync(c => c.MaBenhNhan == benhNhan.MaBenhNhan && !c.DaThanhToan);
                                    
                                if (chiPhiHienTai == null)
                                {
                                    chiPhiHienTai = new ChiPhiDieuTri
                                    {
                                        MaBenhNhan = benhNhan.MaBenhNhan,
                                        TongChiPhi = hinhThucDieuTri.ChiPhi,
                                        DaThanhToan = false,
                                        NgayLap = DateTime.Now
                                    };
                                    _context.ChiPhiDieuTris.Add(chiPhiHienTai);
                                }
                                else
                                {
                                    chiPhiHienTai.TongChiPhi += hinhThucDieuTri.ChiPhi;
                                }
                            }
                        }
                        
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"Đã thêm {viewModel.HinhThucDieuTriIds.Count} hình thức điều trị cho bệnh nhân {benhNhan.MaBenhNhan}");
                    }
                    
                    TempData["SuccessMessage"] = "Cập nhật bệnh nhân thành công!";
                    return RedirectToAction(nameof(Details), new { id = benhNhan.MaBenhNhan });
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!await _context.BenhNhans.AnyAsync(e => e.MaBenhNhan == id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError(ex, $"Lỗi khi cập nhật bệnh nhân ID={id}");
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Lỗi không xác định khi cập nhật bệnh nhân ID={id}");
                    TempData["ErrorMessage"] = "Đã xảy ra lỗi khi cập nhật bệnh nhân. Vui lòng thử lại.";
                }
            }
            
            // Nếu có lỗi, tạo lại các selectlist và trả về view
            viewModel.KhoaSelectList = new SelectList(_context.Khoas, "MaKhoa", "TenKhoa", viewModel.MaKhoa);
            viewModel.BacSiSelectList = new SelectList(
                await _context.BacSis.Where(b => b.MaKhoa == viewModel.MaKhoa).ToListAsync(), 
                "MaBacSi", "HoTen", viewModel.MaBacSi);
            viewModel.HinhThucDieuTriSelectList = new SelectList(
                await _context.HinhThucDieuTris.Where(h => h.MaKhoa == viewModel.MaKhoa).ToListAsync(), 
                "MaDieuTri", "TenDieuTri");
                
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var benhNhan = await _context.BenhNhans
                .Include(b => b.Khoa)
                .Include(b => b.BacSi)
                .Include(b => b.DieuTriBenhNhans)
                .Include(b => b.ChiPhiDieuTris)
                .Include(b => b.LichThamBenhs)
                .FirstOrDefaultAsync(m => m.MaBenhNhan == id);
                
            if (benhNhan == null)
            {
                return NotFound();
            }

            // Kiểm tra xem bệnh nhân có dữ liệu liên quan không
            ViewBag.CoThongTinLienQuan = false;
            ViewBag.ThongBao = "";
            
            if (benhNhan.DieuTriBenhNhans?.Count > 0 || 
                benhNhan.ChiPhiDieuTris?.Count > 0 || 
                benhNhan.LichThamBenhs?.Count > 0)
            {
                ViewBag.CoThongTinLienQuan = true;
                ViewBag.ThongBao = "Bệnh nhân này đã có thông tin điều trị, chi phí hoặc lịch khám. Bạn có chắc chắn muốn xóa?";
            }

            return View(benhNhan);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var benhNhan = await _context.BenhNhans
                    .Include(b => b.DieuTriBenhNhans)
                    .Include(b => b.ChiPhiDieuTris)
                    .Include(b => b.LichThamBenhs)
                    .FirstOrDefaultAsync(b => b.MaBenhNhan == id);
                    
                if (benhNhan == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy bệnh nhân để xóa.";
                    return RedirectToAction(nameof(Index));
                }
                
                // Xóa tất cả dữ liệu liên quan
                if (benhNhan.DieuTriBenhNhans?.Count > 0)
                {
                    _context.DieuTriBenhNhans.RemoveRange(benhNhan.DieuTriBenhNhans);
                }
                
                if (benhNhan.ChiPhiDieuTris?.Count > 0)
                {
                    _context.ChiPhiDieuTris.RemoveRange(benhNhan.ChiPhiDieuTris);
                }
                
                if (benhNhan.LichThamBenhs?.Count > 0)
                {
                    _context.LichThamBenhs.RemoveRange(benhNhan.LichThamBenhs);
                }
                
                // Xóa bệnh nhân
                _context.BenhNhans.Remove(benhNhan);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Đã xóa bệnh nhân thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi xóa bệnh nhân ID={id}");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xóa bệnh nhân. " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}