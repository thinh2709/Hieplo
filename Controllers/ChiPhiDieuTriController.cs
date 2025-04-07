using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBenhVienNoiTru.Data;
using QuanLyBenhVienNoiTru.Models;
using QuanLyBenhVienNoiTru.Filters;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyBenhVienNoiTru.Controllers
{
    [AuthorizeRoles("Admin", "Bác sĩ")]
    public class ChiPhiDieuTriController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChiPhiDieuTriController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchTerm, bool? trangThaiThanhToan, DateTime? tuNgay, DateTime? denNgay)
        {
            IQueryable<ChiPhiDieuTri> query = _context.ChiPhiDieuTris
                .Include(c => c.BenhNhan);
            
            // Lọc theo trạng thái thanh toán
            if (trangThaiThanhToan.HasValue)
            {
                query = query.Where(c => c.DaThanhToan == trangThaiThanhToan.Value);
            }
            
            // Lọc theo ngày lập
            if (tuNgay.HasValue)
            {
                query = query.Where(c => c.NgayLap >= tuNgay.Value);
            }
            
            if (denNgay.HasValue)
            {
                // Thêm 1 ngày để bao gồm hết ngày kết thúc
                DateTime endDate = denNgay.Value.AddDays(1).AddSeconds(-1);
                query = query.Where(c => c.NgayLap <= endDate);
            }
            
            // Tìm kiếm theo tên bệnh nhân hoặc mã chi phí
            if (!string.IsNullOrEmpty(searchTerm))
            {
                // Nếu searchTerm có thể chuyển đổi thành số, tìm theo mã
                if (int.TryParse(searchTerm, out int maChiPhi))
                {
                    query = query.Where(c => c.MaChiPhi == maChiPhi);
                }
                else
                {
                    // Tìm theo tên bệnh nhân (không phân biệt hoa/thường)
                    query = query.Where(c => c.BenhNhan.HoTen.Contains(searchTerm));
                }
            }
            
            // Lưu lại các giá trị tìm kiếm để hiển thị lại trên form
            ViewBag.SearchTerm = searchTerm;
            ViewBag.TrangThaiThanhToan = trangThaiThanhToan;
            ViewBag.TuNgay = tuNgay?.ToString("yyyy-MM-dd");
            ViewBag.DenNgay = denNgay?.ToString("yyyy-MM-dd");
            
            // Sắp xếp theo ngày lập giảm dần (mới nhất lên đầu)
            var chiPhiList = await query.OrderByDescending(c => c.NgayLap).ToListAsync();
            
            return View(chiPhiList);
        }

        public async Task<IActionResult> Details(int? id, bool? backToPatient = false)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chiPhi = await _context.ChiPhiDieuTris
                .Include(c => c.BenhNhan)
                    .ThenInclude(b => b.Khoa)
                .FirstOrDefaultAsync(c => c.MaChiPhi == id);
                
            if (chiPhi == null)
            {
                return NotFound();
            }

            // Lưu mã bệnh nhân để sử dụng cho nút quay lại
            ViewBag.MaBenhNhan = chiPhi.MaBenhNhan;
            ViewBag.BackToPatient = backToPatient ?? false;

            // Lấy danh sách các điều trị của bệnh nhân
            var dieuTriList = await _context.DieuTriBenhNhans
                .Where(d => d.MaBenhNhan == chiPhi.MaBenhNhan)
                .Include(d => d.HinhThucDieuTri)
                .Include(d => d.BacSi)
                .ToListAsync();
                
            ViewBag.DieuTriList = dieuTriList;
            
            // Tính toán chi phí nếu bệnh nhân có bảo hiểm y tế (giảm 80%)
            if (chiPhi.BenhNhan != null && chiPhi.BenhNhan.BaoHiemYTe)
            {
                decimal chiPhiGoc = chiPhi.TongChiPhi;
                decimal giamGia = chiPhiGoc * 0.8m; // Giảm 80%
                decimal chiPhiSauGiamGia = chiPhiGoc * 0.2m; // Chỉ trả 20%
                
                ViewBag.ChiPhiGoc = chiPhiGoc;
                ViewBag.GiamGia = giamGia;
                ViewBag.ChiPhiSauGiamGia = chiPhiSauGiamGia;
                ViewBag.CoBaoHiem = true;
            }
            else
            {
                ViewBag.CoBaoHiem = false;
            }
            
            return View(chiPhi);
        }

        public IActionResult Create(int? maBenhNhan)
        {
            TempData["Info"] = "Chi phí điều trị sẽ được tính tự động thông qua chức năng thêm hình thức điều trị. Vui lòng sử dụng chức năng 'Thêm điều trị' cho bệnh nhân.";
            
            if (maBenhNhan.HasValue)
            {
                return RedirectToAction("Create", "DieuTriBenhNhan", new { maBenhNhan = maBenhNhan });
            }
            
            return RedirectToAction("Index", "DieuTriBenhNhan");
        }

        public async Task<IActionResult> ThanhToan(int? id, bool? backToPatient = false)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chiPhi = await _context.ChiPhiDieuTris
                .Include(c => c.BenhNhan)
                .FirstOrDefaultAsync(c => c.MaChiPhi == id);
                
            if (chiPhi == null)
            {
                return NotFound();
            }

            // Lưu mã bệnh nhân để sử dụng cho nút quay lại
            ViewBag.MaBenhNhan = chiPhi.MaBenhNhan;
            ViewBag.BackToPatient = backToPatient ?? false;

            if (chiPhi.DaThanhToan)
            {
                TempData["Message"] = "Chi phí này đã được thanh toán trước đó.";
                return RedirectToAction(nameof(Details), new { id, backToPatient });
            }
            
            // Tính toán chi phí nếu bệnh nhân có bảo hiểm y tế (giảm 80%)
            if (chiPhi.BenhNhan != null && chiPhi.BenhNhan.BaoHiemYTe)
            {
                decimal chiPhiGoc = chiPhi.TongChiPhi;
                decimal giamGia = chiPhiGoc * 0.8m; // Giảm 80%
                decimal chiPhiSauGiamGia = chiPhiGoc * 0.2m; // Chỉ trả 20%
                
                ViewBag.ChiPhiGoc = chiPhiGoc;
                ViewBag.GiamGia = giamGia;
                ViewBag.ChiPhiSauGiamGia = chiPhiSauGiamGia;
                ViewBag.CoBaoHiem = true;
            }
            else
            {
                ViewBag.CoBaoHiem = false;
            }
            
            return View(chiPhi);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThanhToanConfirmed(int id, bool backToPatient = false)
        {
            var chiPhi = await _context.ChiPhiDieuTris
                .Include(c => c.BenhNhan)
                .FirstOrDefaultAsync(c => c.MaChiPhi == id);
            
            if (chiPhi == null)
            {
                return NotFound();
            }

            chiPhi.DaThanhToan = true;
            _context.Update(chiPhi);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Thanh toán thành công!";
            
            // Sau khi thanh toán, nếu yêu cầu quay lại trang chi tiết bệnh nhân thì chuyển hướng
            if (backToPatient && chiPhi.MaBenhNhan.HasValue)
            {
                return RedirectToAction("Details", "BenhNhan", new { id = chiPhi.MaBenhNhan.Value });
            }
            
            // Ngược lại quay về trang danh sách chi phí
            return RedirectToAction(nameof(Index));
        }
    }
}