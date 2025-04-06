using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyBenhVienNoiTru.Data;
using QuanLyBenhVienNoiTru.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace QuanLyBenhVienNoiTru.Controllers
{
    [Authorize(Roles = "Admin,Nhân viên")]
    public class GiuongController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GiuongController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Giuong
        public async Task<IActionResult> Index()
        {
            var giuongs = await _context.Giuongs
                .Include(g => g.Khoa)
                .Include(g => g.BenhNhan)
                .ToListAsync();
                
            // Chuẩn bị dữ liệu cho filter form
            ViewBag.MaKhoa = new SelectList(_context.Khoas, "MaKhoa", "TenKhoa");
            ViewBag.TrangThaiOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "Tất cả" },
                new SelectListItem { Value = "Trống", Text = "Trống" },
                new SelectListItem { Value = "Đã sử dụng", Text = "Đã sử dụng" },
                new SelectListItem { Value = "Đang sửa chữa", Text = "Đang sửa chữa" }
            };
            
            // Tính toán các thống kê
            ViewBag.TotalCount = giuongs.Count;
            ViewBag.EmptyCount = giuongs.Count(g => g.TrangThai == "Trống");
            ViewBag.OccupiedCount = giuongs.Count(g => g.TrangThai == "Đã sử dụng");
            ViewBag.MaintenanceCount = giuongs.Count(g => g.TrangThai == "Đang sửa chữa");
                
            return View(giuongs);
        }

        // GET: Giuong/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var giuong = await _context.Giuongs
                .Include(g => g.Khoa)
                .Include(g => g.BenhNhan)
                .FirstOrDefaultAsync(m => m.MaGiuong == id);
            if (giuong == null)
            {
                return NotFound();
            }

            return View(giuong);
        }

        // GET: Giuong/Create
        public IActionResult Create()
        {
            ViewBag.MaKhoa = new SelectList(_context.Khoas, "MaKhoa", "TenKhoa");
            ViewBag.TrangThaiOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "Trống", Text = "Trống" },
                new SelectListItem { Value = "Đã sử dụng", Text = "Đã sử dụng" },
                new SelectListItem { Value = "Đang sửa chữa", Text = "Đang sửa chữa" }
            };
            return View();
        }

        // POST: Giuong/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenGiuong,GiaTheoNgay,TrangThai,MaKhoa,MoTa")] Giuong giuong)
        {
            // Bỏ qua lỗi validation liên quan đến navigation properties
            ModelState.Remove("Khoa");
            ModelState.Remove("BenhNhan");
            
            // Bỏ qua lỗi validation cho các thuộc tính con của navigation properties
            foreach (var key in ModelState.Keys.Where(k => k.StartsWith("Khoa.") || k.StartsWith("BenhNhan.")).ToList())
            {
                ModelState.Remove(key);
            }

            // Kiểm tra trạng thái ModelState và in ra danh sách lỗi
            if (!ModelState.IsValid)
            {
                foreach (var key in ModelState.Keys)
                {
                    foreach (var error in ModelState[key].Errors)
                    {
                        // Ghi nhận lỗi vào TempData để hiển thị
                        TempData[$"ModelError_{key}"] = error.ErrorMessage;
                    }
                }
            }

            // Kiểm tra dữ liệu
            if (string.IsNullOrEmpty(giuong.TenGiuong))
            {
                ModelState.AddModelError("TenGiuong", "Tên giường không được để trống");
            }

            if (giuong.MaKhoa <= 0)
            {
                ModelState.AddModelError("MaKhoa", "Vui lòng chọn khoa");
            }

            if (string.IsNullOrEmpty(giuong.TrangThai))
            {
                ModelState.AddModelError("TrangThai", "Vui lòng chọn trạng thái");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Đảm bảo dữ liệu hợp lệ
                    if (giuong.TrangThai == null)
                    {
                        giuong.TrangThai = "Trống";
                    }
                    
                    if (string.IsNullOrEmpty(giuong.MoTa))
                    {
                        giuong.MoTa = "";
                    }
                    
                    // Đảm bảo MaBenhNhan là null khi tạo mới giường
                    giuong.MaBenhNhan = null;
                    
                    // Không thiết lập thuộc tính navigation trực tiếp
                    giuong.Khoa = null;
                    giuong.BenhNhan = null;
                    
                    _context.Add(giuong);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Giường đã được tạo thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Ghi lỗi vào TempData để hiển thị
                    TempData["ExceptionError"] = ex.Message;
                    if (ex.InnerException != null)
                    {
                        TempData["InnerExceptionError"] = ex.InnerException.Message;
                    }
                    ModelState.AddModelError("", "Có lỗi xảy ra khi lưu dữ liệu: " + ex.Message);
                }
            }
            
            ViewBag.MaKhoa = new SelectList(_context.Khoas, "MaKhoa", "TenKhoa", giuong.MaKhoa);
            ViewBag.TrangThaiOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "Trống", Text = "Trống" },
                new SelectListItem { Value = "Đã sử dụng", Text = "Đã sử dụng" },
                new SelectListItem { Value = "Đang sửa chữa", Text = "Đang sửa chữa" }
            };
            return View(giuong);
        }

        // GET: Giuong/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var giuong = await _context.Giuongs.FindAsync(id);
            if (giuong == null)
            {
                return NotFound();
            }
            ViewBag.MaKhoa = new SelectList(_context.Khoas, "MaKhoa", "TenKhoa", giuong.MaKhoa);
            ViewBag.TrangThaiOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "Trống", Text = "Trống" },
                new SelectListItem { Value = "Đã sử dụng", Text = "Đã sử dụng" },
                new SelectListItem { Value = "Đang sửa chữa", Text = "Đang sửa chữa" }
            };
            return View(giuong);
        }

        // POST: Giuong/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaGiuong,TenGiuong,GiaTheoNgay,TrangThai,MaKhoa,MaBenhNhan,MoTa")] Giuong giuong)
        {
            if (id != giuong.MaGiuong)
            {
                return NotFound();
            }

            // Bỏ qua lỗi validation liên quan đến navigation properties
            ModelState.Remove("Khoa");
            ModelState.Remove("BenhNhan");
            
            // Bỏ qua lỗi validation cho các thuộc tính con của navigation properties
            foreach (var key in ModelState.Keys.Where(k => k.StartsWith("Khoa.") || k.StartsWith("BenhNhan.")).ToList())
            {
                ModelState.Remove(key);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra nếu trạng thái đã thay đổi từ "Đã sử dụng" sang khác
                    // và nếu có BenhNhan, thì cần xoá liên kết
                    var currentGiuong = await _context.Giuongs
                        .AsNoTracking()
                        .FirstOrDefaultAsync(g => g.MaGiuong == id);
                    
                    if (currentGiuong.TrangThai == "Đã sử dụng" && 
                        giuong.TrangThai != "Đã sử dụng" && 
                        currentGiuong.MaBenhNhan.HasValue)
                    {
                        giuong.MaBenhNhan = null;
                    }

                    // Không thiết lập thuộc tính navigation trực tiếp
                    giuong.Khoa = null;
                    giuong.BenhNhan = null;

                    _context.Update(giuong);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Giường đã được cập nhật thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GiuongExists(giuong.MaGiuong))
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
            ViewBag.MaKhoa = new SelectList(_context.Khoas, "MaKhoa", "TenKhoa", giuong.MaKhoa);
            ViewBag.TrangThaiOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "Trống", Text = "Trống" },
                new SelectListItem { Value = "Đã sử dụng", Text = "Đã sử dụng" },
                new SelectListItem { Value = "Đang sửa chữa", Text = "Đang sửa chữa" }
            };
            return View(giuong);
        }

        // GET: Giuong/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var giuong = await _context.Giuongs
                .Include(g => g.Khoa)
                .Include(g => g.BenhNhan)
                .FirstOrDefaultAsync(m => m.MaGiuong == id);
            if (giuong == null)
            {
                return NotFound();
            }

            return View(giuong);
        }

        // POST: Giuong/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var giuong = await _context.Giuongs.FindAsync(id);
            if (giuong != null)
            {
                // Kiểm tra xem giường có đang được sử dụng không
                if (giuong.TrangThai == "Đã sử dụng" || giuong.MaBenhNhan != null)
                {
                    TempData["ErrorMessage"] = "Không thể xóa giường đang được sử dụng!";
                    return RedirectToAction(nameof(Index));
                }
                
                _context.Giuongs.Remove(giuong);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Giường đã được xóa thành công!";
            }
            
            return RedirectToAction(nameof(Index));
        }

        // GET: Giuong/AssignPatient/5
        public async Task<IActionResult> AssignPatient(int? id, int? maKhoa)
        {
            if (id == null)
            {
                return NotFound();
            }

            var giuong = await _context.Giuongs
                .Include(g => g.Khoa)
                .FirstOrDefaultAsync(m => m.MaGiuong == id);
            
            if (giuong == null)
            {
                return NotFound();
            }

            if (giuong.TrangThai != "Trống")
            {
                TempData["ErrorMessage"] = "Chỉ có thể phân bệnh nhân vào giường trống!";
                return RedirectToAction(nameof(Index));
            }

            // Lấy danh sách tất cả các khoa
            ViewBag.Khoas = new SelectList(await _context.Khoas.ToListAsync(), "MaKhoa", "TenKhoa", giuong.MaKhoa);
            
            // Nếu không chọn khoa cụ thể, mặc định là khoa của giường
            int selectedKhoa = maKhoa ?? giuong.MaKhoa;

            // Lấy danh sách bệnh nhân thuộc khoa đã chọn và chưa có giường
            var availablePatients = await _context.BenhNhans
                .Where(bn => bn.MaKhoa == selectedKhoa && !_context.Giuongs.Any(g => g.MaBenhNhan == bn.MaBenhNhan))
                .ToListAsync();

            ViewBag.MaBenhNhan = new SelectList(availablePatients, "MaBenhNhan", "HoTen");
            ViewBag.Giuong = giuong;
            ViewBag.SelectedKhoa = selectedKhoa;
            
            return View();
        }

        // POST: Giuong/AssignPatient/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignPatient(int id, int maBenhNhan)
        {
            var giuong = await _context.Giuongs.FindAsync(id);
            if (giuong == null)
            {
                return NotFound();
            }

            // Kiểm tra xem giường có trống không
            if (giuong.TrangThai != "Trống")
            {
                TempData["ErrorMessage"] = "Chỉ có thể phân bệnh nhân vào giường trống!";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra xem bệnh nhân đã có giường khác chưa
            var existingBed = await _context.Giuongs.FirstOrDefaultAsync(g => g.MaBenhNhan == maBenhNhan);
            if (existingBed != null)
            {
                TempData["ErrorMessage"] = "Bệnh nhân này đã được phân vào giường khác!";
                return RedirectToAction(nameof(AssignPatient), new { id });
            }

            // Phân bệnh nhân vào giường
            giuong.MaBenhNhan = maBenhNhan;
            giuong.TrangThai = "Đã sử dụng";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã phân bệnh nhân vào giường thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Giuong/ReleasePatient/5
        public async Task<IActionResult> ReleasePatient(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var giuong = await _context.Giuongs
                .Include(g => g.Khoa)
                .Include(g => g.BenhNhan)
                .FirstOrDefaultAsync(m => m.MaGiuong == id);
            
            if (giuong == null)
            {
                return NotFound();
            }

            if (giuong.TrangThai != "Đã sử dụng" || giuong.MaBenhNhan == null)
            {
                TempData["ErrorMessage"] = "Giường này không có bệnh nhân để xuất!";
                return RedirectToAction(nameof(Index));
            }
            
            return View(giuong);
        }

        // POST: Giuong/ReleasePatient/5
        [HttpPost, ActionName("ReleasePatient")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReleasePatientConfirmed(int id)
        {
            var giuong = await _context.Giuongs.FindAsync(id);
            if (giuong == null)
            {
                return NotFound();
            }

            if (giuong.TrangThai != "Đã sử dụng" || giuong.MaBenhNhan == null)
            {
                TempData["ErrorMessage"] = "Giường này không có bệnh nhân để xuất!";
                return RedirectToAction(nameof(Index));
            }

            // Giải phóng giường
            giuong.MaBenhNhan = null;
            giuong.TrangThai = "Trống";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xuất bệnh nhân khỏi giường thành công!";
            return RedirectToAction(nameof(Index));
        }

        private bool GiuongExists(int id)
        {
            return _context.Giuongs.Any(e => e.MaGiuong == id);
        }

        // GET: Giuong/FilterBeds
        public async Task<IActionResult> FilterBeds(int? maKhoa, string trangThai)
        {
            var giuongsQuery = _context.Giuongs
                .Include(g => g.Khoa)
                .Include(g => g.BenhNhan)
                .AsQueryable();

            if (maKhoa.HasValue)
            {
                giuongsQuery = giuongsQuery.Where(g => g.MaKhoa == maKhoa.Value);
            }

            if (!string.IsNullOrEmpty(trangThai))
            {
                giuongsQuery = giuongsQuery.Where(g => g.TrangThai == trangThai);
            }

            var khoas = await _context.Khoas.ToListAsync();
            
            ViewBag.MaKhoa = new SelectList(khoas, "MaKhoa", "TenKhoa", maKhoa);
            ViewBag.TrangThaiOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "Tất cả" },
                new SelectListItem { Value = "Trống", Text = "Trống" },
                new SelectListItem { Value = "Đã sử dụng", Text = "Đã sử dụng" },
                new SelectListItem { Value = "Đang sửa chữa", Text = "Đang sửa chữa" }
            };
            
            ViewBag.SelectedTrangThai = trangThai;
            
            var filteredGiuongs = await giuongsQuery.ToListAsync();
            
            ViewBag.TotalCount = filteredGiuongs.Count;
            ViewBag.EmptyCount = filteredGiuongs.Count(g => g.TrangThai == "Trống");
            ViewBag.OccupiedCount = filteredGiuongs.Count(g => g.TrangThai == "Đã sử dụng");
            ViewBag.MaintenanceCount = filteredGiuongs.Count(g => g.TrangThai == "Đang sửa chữa");

            return View("Index", filteredGiuongs);
        }

        // GET: Giuong/BedReport
        public async Task<IActionResult> BedReport()
        {
            var bedReportByDepartment = await _context.Khoas
                .Where(k => k.TrangThai)
                .Select(k => new
                {
                    Department = k,
                    TotalBeds = _context.Giuongs.Count(g => g.MaKhoa == k.MaKhoa),
                    EmptyBeds = _context.Giuongs.Count(g => g.MaKhoa == k.MaKhoa && g.TrangThai == "Trống"),
                    OccupiedBeds = _context.Giuongs.Count(g => g.MaKhoa == k.MaKhoa && g.TrangThai == "Đã sử dụng"),
                    MaintenanceBeds = _context.Giuongs.Count(g => g.MaKhoa == k.MaKhoa && g.TrangThai == "Đang sửa chữa"),
                })
                .ToListAsync();

            var reportData = bedReportByDepartment.Select(report => new
            {
                DepartmentName = report.Department.TenKhoa,
                DepartmentId = report.Department.MaKhoa,
                TotalBeds = report.TotalBeds,
                EmptyBeds = report.EmptyBeds,
                OccupiedBeds = report.OccupiedBeds,
                MaintenanceBeds = report.MaintenanceBeds,
                OccupancyRate = report.TotalBeds > 0 ? (double)report.OccupiedBeds / report.TotalBeds * 100 : 0
            }).ToList();

            ViewBag.TotalBeds = reportData.Sum(r => r.TotalBeds);
            ViewBag.TotalEmptyBeds = reportData.Sum(r => r.EmptyBeds);
            ViewBag.TotalOccupiedBeds = reportData.Sum(r => r.OccupiedBeds);
            ViewBag.TotalMaintenanceBeds = reportData.Sum(r => r.MaintenanceBeds);
            ViewBag.OverallOccupancyRate = ViewBag.TotalBeds > 0 
                ? (double)ViewBag.TotalOccupiedBeds / ViewBag.TotalBeds * 100 
                : 0;

            return View(reportData);
        }
        
        // GET: Giuong/BedUsageDetails/5
        public async Task<IActionResult> BedUsageDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var khoa = await _context.Khoas.FindAsync(id);
            if (khoa == null)
            {
                return NotFound();
            }

            var bedsInDepartment = await _context.Giuongs
                .Include(g => g.BenhNhan)
                .Where(g => g.MaKhoa == id)
                .ToListAsync();

            ViewBag.Khoa = khoa;
            return View(bedsInDepartment);
        }

        // API để lấy danh sách bệnh nhân theo khoa
        [HttpGet]
        public async Task<JsonResult> GetPatientsByDepartment(int maKhoa)
        {
            // Lấy danh sách bệnh nhân thuộc khoa và chưa có giường
            var patients = await _context.BenhNhans
                .Where(bn => bn.MaKhoa == maKhoa && !_context.Giuongs.Any(g => g.MaBenhNhan == bn.MaBenhNhan))
                .Select(p => new { id = p.MaBenhNhan, text = p.HoTen })
                .ToListAsync();
                
            return Json(patients);
        }
    }
} 