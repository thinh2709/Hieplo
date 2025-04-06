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

            // Lấy thông tin giường bệnh nếu có
            var giuong = await _context.Giuongs
                .FirstOrDefaultAsync(g => g.MaBenhNhan == id);

            // Tạo view model cho chi tiết bệnh nhân
            var viewModel = new BenhNhanDetailsViewModel
            {
                BenhNhan = benhNhan,
                DieuTris = benhNhan.DieuTriBenhNhans?.ToList() ?? new List<DieuTriBenhNhan>(),
                ChiPhis = benhNhan.ChiPhiDieuTris?.ToList() ?? new List<ChiPhiDieuTri>(),
                LichThamBenhs = benhNhan.LichThamBenhs?.ToList() ?? new List<LichThamBenh>(),
                TongChiPhiChuaThanhToan = benhNhan.ChiPhiDieuTris?.Where(c => !c.DaThanhToan).Sum(c => c.TongChiPhi) ?? 0,
                TongChiPhiDaThanhToan = benhNhan.ChiPhiDieuTris?.Where(c => c.DaThanhToan).Sum(c => c.TongChiPhi) ?? 0,
                Giuong = giuong
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
            
            // Khởi tạo danh sách giường trống
            viewModel.GiuongSelectList = new SelectList(new List<SelectListItem>());
            
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BenhNhanCreateWithTreatmentViewModel viewModel)
        {
            try
            {
                // Log the received data
                _logger.LogInformation("Received data for new patient: " + 
                    $"HoTen={viewModel.BenhNhan?.HoTen}, " +
                    $"MaKhoa={viewModel.BenhNhan?.MaKhoa}, " +
                    $"GioiTinh={viewModel.BenhNhan?.GioiTinh}, " +
                    $"ThemDieuTri={viewModel.ThemDieuTri}, " +
                    $"MaBacSi={viewModel.MaBacSi}, " +
                    $"Selected treatments={string.Join(",", viewModel.HinhThucDieuTriIds?.Select(i => i.ToString()) ?? Enumerable.Empty<string>())}");

                // Loại bỏ validation errors cho các navigation properties
                foreach (var key in ModelState.Keys.Where(k => k.StartsWith("BenhNhan.Khoa") || 
                                                           k.StartsWith("BenhNhan.BacSi") || 
                                                           k.StartsWith("BenhNhan.DieuTriBenhNhans") || 
                                                           k.StartsWith("BenhNhan.LichThamBenhs") || 
                                                           k.StartsWith("BenhNhan.ChiPhiDieuTris") || 
                                                           k.StartsWith("BenhNhan.Giuong") ||
                                                           k.StartsWith("BenhNhan.GiuongMaGiuong") ||
                                                           k == "GiuongMaGiuong" ||
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

                    _logger.LogError($"Validation errors when adding new patient: {validationErrors}");
                    
                    // Log detailed info about each property with errors
                    foreach (var entry in ModelState.Where(e => e.Value.Errors.Any()))
                    {
                        _logger.LogError($"Property {entry.Key} has errors: {string.Join(", ", entry.Value.Errors.Select(e => e.ErrorMessage))}");
                    }
                    
                    // Prepare detailed validation errors for the view
                    var errorList = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .Select(x => new { 
                            Property = x.Key, 
                            Errors = x.Value.Errors.Select(e => e.ErrorMessage).ToList() 
                        })
                        .ToList();
                    
                    ViewBag.ValidationErrors = errorList;
                    ViewBag.FullErrorDetails = "ModelState keys: " + string.Join(", ", ModelState.Keys);
                    
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
                    
                    // Lấy danh sách giường trống theo khoa
                    if (viewModel.BenhNhan?.MaKhoa > 0)
                    {
                        var availableBeds = _context.Giuongs
                            .Where(g => g.MaKhoa == viewModel.BenhNhan.MaKhoa && g.TrangThai == "Trống")
                            .Select(g => new
                            {
                                MaGiuong = g.MaGiuong,
                                TenGiuong = $"{g.TenGiuong} - {g.GiaTheoNgay:N0} VND/ngày"
                            })
                            .ToList();
                        
                        viewModel.GiuongSelectList = new SelectList(availableBeds, "MaGiuong", "TenGiuong", viewModel.MaGiuong);
                    }
                    else
                    {
                        viewModel.GiuongSelectList = new SelectList(new List<SelectListItem>());
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
                    TrangThai = viewModel.BenhNhan.TrangThai,
                    // Thiết lập GiuongMaGiuong là giá trị mặc định hoặc giường được chọn
                    GiuongMaGiuong = viewModel.MaGiuong ?? 0
                };

                // Thêm bệnh nhân vào database
                _context.BenhNhans.Add(newBenhNhan);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Đã thêm bệnh nhân thành công: {newBenhNhan.MaBenhNhan}");
                
                // Nếu đã chọn giường, cập nhật thông tin giường
                if (viewModel.MaGiuong.HasValue && viewModel.MaGiuong.Value > 0)
                {
                    try {
                        var giuong = await _context.Giuongs.FindAsync(viewModel.MaGiuong.Value);
                        if (giuong != null && giuong.TrangThai == "Trống")
                        {
                            giuong.MaBenhNhan = newBenhNhan.MaBenhNhan;
                            giuong.TrangThai = "Đã sử dụng";
                            
                            // Cập nhật lại trường GiuongMaGiuong của bệnh nhân
                            newBenhNhan.GiuongMaGiuong = giuong.MaGiuong;
                            _context.Update(newBenhNhan);
                            
                            await _context.SaveChangesAsync();
                            
                            _logger.LogInformation($"Đã phân bệnh nhân {newBenhNhan.MaBenhNhan} vào giường {giuong.MaGiuong}");
                        }
                    } catch (Exception ex) {
                        _logger.LogError($"Lỗi khi cập nhật giường: {ex.Message}");
                    }
                }
                
                // Nếu người dùng chọn thêm hình thức điều trị
                if (viewModel.ThemDieuTri && viewModel.HinhThucDieuTriIds?.Count > 0)
                {
                    _logger.LogInformation($"Adding treatments: {string.Join(", ", viewModel.HinhThucDieuTriIds)}");
                    _logger.LogInformation($"Bác sĩ phụ trách ID: {viewModel.MaBacSi}");
                    
                    try
                    {
                        // Ensure the list is not null
                        if (viewModel.HinhThucDieuTriIds == null)
                        {
                            viewModel.HinhThucDieuTriIds = new List<int>();
                            _logger.LogWarning("HinhThucDieuTriIds was null, initialized to empty list");
                        }
                        
                        // Thêm các hình thức điều trị được chọn
                        foreach (var dieuTriId in viewModel.HinhThucDieuTriIds)
                        {
                            _logger.LogInformation($"Processing treatment ID: {dieuTriId}");
                            
                            var dieuTri = new DieuTriBenhNhan
                            {
                                MaBenhNhan = newBenhNhan.MaBenhNhan,
                                MaDieuTri = dieuTriId,
                                MaBacSi = viewModel.MaBacSi,
                                NgayThucHien = viewModel.NgayThucHien ?? DateTime.Now
                            };
                            
                            if (dieuTri.MaBacSi == null || dieuTri.MaBacSi == 0)
                            {
                                _logger.LogWarning($"Bác sĩ không được chọn cho điều trị ID: {dieuTriId}");
                                if (User.IsInRole("Bác sĩ"))
                                {
                                    // Tự động lấy bác sĩ hiện tại nếu người dùng là bác sĩ
                                    var currentUser = await _context.TaiKhoans
                                        .Include(t => t.BacSi)
                                        .FirstOrDefaultAsync(t => t.TenDangNhap == User.Identity.Name);
                                        
                                    if (currentUser?.BacSi != null)
                                    {
                                        dieuTri.MaBacSi = currentUser.BacSi.MaBacSi;
                                        _logger.LogInformation($"Tự động chọn bác sĩ hiện tại ID: {dieuTri.MaBacSi}");
                                    }
                                }
                            }
                            
                            _context.DieuTriBenhNhans.Add(dieuTri);
                            await _context.SaveChangesAsync(); // Lưu điều trị trước khi cập nhật chi phí
                            
                            // Cập nhật chi phí
                            var hinhThucDieuTri = await _context.HinhThucDieuTris.FindAsync(dieuTriId);
                            
                            if (hinhThucDieuTri != null)
                            {
                                _logger.LogInformation($"Found treatment: {hinhThucDieuTri.TenDieuTri} with cost: {hinhThucDieuTri.ChiPhi}");
                                
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
                                    _logger.LogInformation($"Created new cost entry with initial cost: {hinhThucDieuTri.ChiPhi}");
                                }
                                else
                                {
                                    chiPhiHienTai.TongChiPhi += hinhThucDieuTri.ChiPhi;
                                    _logger.LogInformation($"Updated existing cost entry, new total: {chiPhiHienTai.TongChiPhi}");
                                }
                                
                                await _context.SaveChangesAsync();
                            }
                            else
                            {
                                _logger.LogWarning($"Treatment with ID {dieuTriId} not found");
                            }
                        }
                        
                        // Cập nhật bác sĩ phụ trách cho bệnh nhân nếu chưa có
                        if (viewModel.MaBacSi.HasValue && viewModel.MaBacSi.Value > 0 && (!newBenhNhan.MaBacSi.HasValue || newBenhNhan.MaBacSi.Value == 0))
                        {
                            newBenhNhan.MaBacSi = viewModel.MaBacSi;
                            _context.Update(newBenhNhan);
                            await _context.SaveChangesAsync();
                            _logger.LogInformation($"Đã cập nhật bác sĩ phụ trách {viewModel.MaBacSi} cho bệnh nhân {newBenhNhan.MaBenhNhan}");
                        }
                        
                        _logger.LogInformation($"Đã thêm {viewModel.HinhThucDieuTriIds.Count} hình thức điều trị cho bệnh nhân {newBenhNhan.MaBenhNhan}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Lỗi khi thêm điều trị: {ex.Message}");
                        if (ex.InnerException != null)
                        {
                            _logger.LogError($"Inner exception: {ex.InnerException.Message}");
                        }
                    }
                }
                else
                {
                    _logger.LogInformation("No treatments added. ThemDieuTri = " + viewModel.ThemDieuTri + 
                        ", HinhThucDieuTriIds count = " + (viewModel.HinhThucDieuTriIds?.Count.ToString() ?? "null"));
                }
                
                TempData["Success"] = $"Thêm bệnh nhân {newBenhNhan.HoTen} thành công!";
                return RedirectToAction(nameof(Details), new { id = newBenhNhan.MaBenhNhan });
            }
            catch (Exception ex)
            {
                // Log exception details
                _logger.LogError(ex, "Error when adding new patient");
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner exception: {ex.InnerException.Message}");
                }

                TempData["ErrorMessage"] = $"Đã xảy ra lỗi khi thêm bệnh nhân: {ex.Message}";
                if (ex.InnerException != null)
                {
                    TempData["ErrorMessage"] += $" Chi tiết: {ex.InnerException.Message}";
                }
                
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
                .FirstOrDefaultAsync(m => m.MaBenhNhan == id);
            
            if (benhNhan == null)
            {
                return NotFound();
            }

            // Lấy thông tin bác sĩ phụ trách bệnh nhân này (nếu có)
            var dieuTriHienTai = await _context.DieuTriBenhNhans
                .Where(d => d.MaBenhNhan == id)
                .OrderByDescending(d => d.NgayThucHien)
                .FirstOrDefaultAsync();

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
                MaBacSi = dieuTriHienTai?.MaBacSi,
                KhoaSelectList = new SelectList(_context.Khoas, "MaKhoa", "TenKhoa", benhNhan.MaKhoa),
                BacSiSelectList = new SelectList(_context.BacSis, "MaBacSi", "HoTen", dieuTriHienTai?.MaBacSi),
                GioiTinhOptions = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Nam", Text = "Nam", Selected = benhNhan.GioiTinh == "Nam" },
                    new SelectListItem { Value = "Nữ", Text = "Nữ", Selected = benhNhan.GioiTinh == "Nữ" }
                }
            };
            
            // Lấy thông tin giường hiện tại của bệnh nhân (nếu có)
            var currentBed = await _context.Giuongs
                .FirstOrDefaultAsync(g => g.MaBenhNhan == id);
                
            viewModel.MaGiuong = currentBed?.MaGiuong;
            
            // Lấy danh sách giường trống theo khoa và thêm cả giường hiện tại của bệnh nhân
            var availableBeds = await _context.Giuongs
                .Where(g => g.MaKhoa == benhNhan.MaKhoa && 
                          (g.TrangThai == "Trống" || g.MaBenhNhan == id))
                .ToListAsync();
                
            var bedSelectItems = availableBeds.Select(g => new
            {
                MaGiuong = g.MaGiuong,
                TenGiuong = $"{g.TenGiuong} - {g.GiaTheoNgay:N0} VND/ngày"
            }).ToList();
                
            viewModel.GiuongSelectList = new SelectList(bedSelectItems, "MaGiuong", "TenGiuong", currentBed?.MaGiuong);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BenhNhanEditViewModel model)
        {
            if (id != model.MaBenhNhan)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var benhNhan = await _context.BenhNhans.FindAsync(id);
                    if (benhNhan == null)
                    {
                        return NotFound();
                    }

                    // Cập nhật thông tin bệnh nhân
                    benhNhan.HoTen = model.HoTen;
                    benhNhan.NgaySinh = model.NgaySinh;
                    benhNhan.GioiTinh = model.GioiTinh;
                    benhNhan.SoDienThoai = model.SoDienThoai;
                    benhNhan.Email = model.Email;
                    benhNhan.DiaChi = model.DiaChi;
                    benhNhan.BaoHiemYTe = model.BaoHiemYTe;
                    benhNhan.NgayNhapVien = model.NgayNhapVien;
                    benhNhan.NgayXuatVien = model.NgayXuatVien;
                    benhNhan.MaKhoa = model.MaKhoa;
                    benhNhan.ChanDoan = model.ChanDoan;
                    benhNhan.TrangThai = model.TrangThai;
                    benhNhan.MaBacSi = model.MaBacSi;

                    // Nếu có thêm điều trị mới
                    if (model.ThemDieuTri && model.HinhThucDieuTriIds != null && model.HinhThucDieuTriIds.Any())
                    {
                        var dieuTri = new DieuTriBenhNhan
                        {
                            MaBenhNhan = model.MaBenhNhan,
                            MaBacSi = model.DieuTriMaBacSi,
                            NgayThucHien = model.NgayThucHien ?? DateTime.Now
                        };

                        _context.DieuTriBenhNhans.Add(dieuTri);
                        await _context.SaveChangesAsync();

                        // Tạo một chi phí điều trị mới tổng hợp
                        var chiPhi = new ChiPhiDieuTri
                        {
                            MaBenhNhan = model.MaBenhNhan,
                            TongChiPhi = 0,
                            DaThanhToan = false,
                            NgayLap = DateTime.Now
                        };
                        
                        _context.ChiPhiDieuTris.Add(chiPhi);

                        // Tính tổng chi phí từ các hình thức điều trị
                        decimal tongChiPhi = 0;
                        foreach (var maDieuTri in model.HinhThucDieuTriIds)
                        {
                            // Lấy thông tin hình thức điều trị
                            var hinhThucDieuTri = await _context.HinhThucDieuTris.FindAsync(maDieuTri);
                            if (hinhThucDieuTri != null)
                            {
                                // Thêm hình thức điều trị vào dieuTri
                                dieuTri.MaDieuTri = maDieuTri;
                                
                                // Cộng dồn chi phí
                                tongChiPhi += hinhThucDieuTri.ChiPhi;
                            }
                        }
                        
                        // Cập nhật tổng chi phí
                        chiPhi.TongChiPhi = tongChiPhi;

                        await _context.SaveChangesAsync();
                    }

                    // Cập nhật thông tin giường
                    if (model.MaGiuong.HasValue)
                    {
                        var giuong = await _context.Giuongs.FindAsync(model.MaGiuong.Value);
                        if (giuong != null)
                        {
                            giuong.MaBenhNhan = model.MaBenhNhan;
                            giuong.TrangThai = "Đã sử dụng";
                            _context.Update(giuong);
                        }
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật thông tin bệnh nhân thành công.";
                    return RedirectToAction(nameof(Details), new { id = model.MaBenhNhan });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BenhNhanExists(model.MaBenhNhan))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Nếu có lỗi, load lại các danh sách select
            model.KhoaSelectList = new SelectList(await _context.Khoas.ToListAsync(), "MaKhoa", "TenKhoa", model.MaKhoa);
            model.GioiTinhOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "Nam", Text = "Nam" },
                new SelectListItem { Value = "Nữ", Text = "Nữ" },
                new SelectListItem { Value = "Khác", Text = "Khác" }
            };
            model.BacSiSelectList = new SelectList(await _context.BacSis.ToListAsync(), "MaBacSi", "HoTen", model.MaBacSi);
            model.HinhThucDieuTriSelectList = new SelectList(await _context.HinhThucDieuTris.ToListAsync(), "MaDieuTri", "TenDieuTri");
            
            // Lấy danh sách giường trống theo khoa và thêm cả giường hiện tại của bệnh nhân
            var availableBeds = await _context.Giuongs
                .Where(g => g.MaKhoa == model.MaKhoa && 
                          (g.TrangThai == "Trống" || g.MaBenhNhan == id))
                .ToListAsync();
                
            var bedSelectItems = availableBeds.Select(g => new
            {
                MaGiuong = g.MaGiuong,
                TenGiuong = $"{g.TenGiuong} - {g.GiaTheoNgay:N0} VND/ngày"
            }).ToList();
                
            model.GiuongSelectList = new SelectList(bedSelectItems, "MaGiuong", "TenGiuong", model.MaGiuong);

            return View(model);
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
                
                // Giải phóng giường bệnh nếu bệnh nhân đang sử dụng
                var giuong = await _context.Giuongs
                    .FirstOrDefaultAsync(g => g.MaBenhNhan == id);
                    
                if (giuong != null)
                {
                    giuong.MaBenhNhan = null;
                    giuong.TrangThai = "Trống";
                    _context.Update(giuong);
                    
                    // Đặt GiuongMaGiuong về 0
                    benhNhan.GiuongMaGiuong = 0;
                    _context.Update(benhNhan);
                    
                    _logger.LogInformation($"Đã giải phóng giường {giuong.MaGiuong} do xóa bệnh nhân {id}");
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

        [HttpGet]
        public async Task<IActionResult> GetGiuongTrongByKhoa(int maKhoa)
        {
            _logger.LogInformation($"Getting empty beds for department ID: {maKhoa}");
            
            var giuongTrong = await _context.Giuongs
                .Where(g => g.MaKhoa == maKhoa && g.TrangThai == "Trống")
                .Select(g => new 
                {
                    maGiuong = g.MaGiuong,
                    tenGiuong = g.TenGiuong,
                    giaTheoNgay = g.GiaTheoNgay
                })
                .ToListAsync();
                
            _logger.LogInformation($"Found {giuongTrong.Count} empty beds for department ID: {maKhoa}");
            
            return Json(giuongTrong);
        }

        [HttpGet]
        public async Task<IActionResult> XuatVien(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var benhNhan = await _context.BenhNhans.FindAsync(id);
            if (benhNhan == null)
            {
                return NotFound();
            }

            if (!benhNhan.TrangThai)
            {
                TempData["ErrorMessage"] = "Bệnh nhân này đã xuất viện trước đó.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var viewModel = new BenhNhanXuatVienViewModel
            {
                MaBenhNhan = benhNhan.MaBenhNhan,
                HoTen = benhNhan.HoTen,
                NgayXuatVien = DateTime.Now
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XuatVien(BenhNhanXuatVienViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var benhNhan = await _context.BenhNhans.FindAsync(viewModel.MaBenhNhan);
            if (benhNhan == null)
            {
                return NotFound();
            }

            try
            {
                // Cập nhật thông tin xuất viện
                benhNhan.TrangThai = false;
                benhNhan.NgayXuatVien = viewModel.NgayXuatVien;
                
                // Ghi chú xuất viện (nếu có)
                if (!string.IsNullOrEmpty(viewModel.GhiChu))
                {
                    benhNhan.ChanDoan += $"\n\n[Ghi chú xuất viện: {viewModel.GhiChu}]";
                }
                
                // Đặt GiuongMaGiuong về 0 khi xuất viện
                benhNhan.GiuongMaGiuong = 0;
                
                _context.Update(benhNhan);
                
                // Giải phóng giường bệnh nếu bệnh nhân đang sử dụng
                var giuong = await _context.Giuongs
                    .FirstOrDefaultAsync(g => g.MaBenhNhan == viewModel.MaBenhNhan);
                    
                if (giuong != null)
                {
                    giuong.MaBenhNhan = null;
                    giuong.TrangThai = "Trống";
                    _context.Update(giuong);
                    
                    _logger.LogInformation($"Đã giải phóng giường {giuong.MaGiuong} do bệnh nhân {viewModel.MaBenhNhan} xuất viện");
                }
                
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = $"Đã xuất viện thành công cho bệnh nhân {benhNhan.HoTen}";
                return RedirectToAction(nameof(Details), new { id = viewModel.MaBenhNhan });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi xuất viện cho bệnh nhân ID={viewModel.MaBenhNhan}");
                ModelState.AddModelError("", "Đã xảy ra lỗi khi xuất viện. Vui lòng thử lại.");
                return View(viewModel);
            }
        }

        private bool BenhNhanExists(int id)
        {
            return _context.BenhNhans.Any(e => e.MaBenhNhan == id);
        }
    }
}