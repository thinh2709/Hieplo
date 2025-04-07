using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBenhVienNoiTru.Data;
using QuanLyBenhVienNoiTru.Models;
using QuanLyBenhVienNoiTru.Filters;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace QuanLyBenhVienNoiTru.Controllers
{
    [AuthorizeRoles("Admin")]
    public class HinhThucDieuTriController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HinhThucDieuTriController> _logger;

        public HinhThucDieuTriController(ApplicationDbContext context, ILogger<HinhThucDieuTriController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string searchString, int? khoaId)
        {
            var query = _context.HinhThucDieuTris
                .Include(h => h.Khoa)
                .AsQueryable();

            // Filter by department if specified
            if (khoaId.HasValue && khoaId > 0)
            {
                query = query.Where(h => h.MaKhoa == khoaId);
                ViewBag.SelectedKhoa = khoaId;
            }

            // Filter by search string if provided
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(h => h.TenDieuTri.Contains(searchString));
                ViewBag.CurrentFilter = searchString;
            }

            // Get departments for the filter dropdown
            ViewBag.Khoas = await _context.Khoas.OrderBy(k => k.TenKhoa).ToListAsync();

            return View(await query.OrderBy(h => h.TenDieuTri).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hinhThucDieuTri = await _context.HinhThucDieuTris
                .Include(h => h.Khoa)
                .FirstOrDefaultAsync(m => m.MaDieuTri == id);
                
            if (hinhThucDieuTri == null)
            {
                return NotFound();
            }

            return View(hinhThucDieuTri);
        }

        public IActionResult Create()
        {
            ViewBag.MaKhoa = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Khoas, "MaKhoa", "TenKhoa");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaDieuTri,TenDieuTri,MaKhoa,ChiPhi,MoTa")] HinhThucDieuTri hinhThucDieuTri)
        {
            // Remove validation errors for navigation properties if any exist
            ModelState.Remove("Khoa");
            ModelState.Remove("DieuTriBenhNhans");
            
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(hinhThucDieuTri);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Thêm mới thành công hình thức điều trị: {hinhThucDieuTri.TenDieuTri}";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi lưu hình thức điều trị.");
                    _logger.LogError(ex, "Lỗi khi thêm mới hình thức điều trị");
                    TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lưu hình thức điều trị. Vui lòng thử lại sau.";
                }
            }
            else
            {
                // Collect validation errors for display
                var validationErrors = new StringBuilder();
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        validationErrors.AppendLine($"• {error.ErrorMessage}");
                    }
                }
                TempData["ValidationErrors"] = validationErrors.ToString();
                
                _logger.LogWarning("Lỗi xác thực biểu mẫu khi thêm mới hình thức điều trị: {Errors}", 
                    string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)));
            }

            ViewData["MaKhoa"] = new SelectList(_context.Khoas, "MaKhoa", "TenKhoa", hinhThucDieuTri.MaKhoa);
            return View(hinhThucDieuTri);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "Không tìm thấy ID hình thức điều trị";
                return RedirectToAction(nameof(Index));
            }

            var hinhThucDieuTri = await _context.HinhThucDieuTris.FindAsync(id);
            if (hinhThucDieuTri == null)
            {
                TempData["Error"] = "Không tìm thấy hình thức điều trị";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.MaKhoa = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Khoas, "MaKhoa", "TenKhoa", hinhThucDieuTri.MaKhoa);
            return View(hinhThucDieuTri);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaDieuTri,TenDieuTri,MaKhoa,ChiPhi,MoTa")] HinhThucDieuTri hinhThucDieuTri)
        {
            if (id != hinhThucDieuTri.MaDieuTri)
            {
                TempData["ErrorMessage"] = $"ID điều trị ({id}) không khớp với dữ liệu được gửi.";
                return RedirectToAction(nameof(Index));
            }

            // Remove validation errors for navigation properties if any exist
            ModelState.Remove("Khoa");
            ModelState.Remove("DieuTriBenhNhans");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(hinhThucDieuTri);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Cập nhật thành công hình thức điều trị: {hinhThucDieuTri.TenDieuTri}";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HinhThucDieuTriExists(hinhThucDieuTri.MaDieuTri))
                    {
                        TempData["ErrorMessage"] = $"Không tìm thấy hình thức điều trị có ID: {hinhThucDieuTri.MaDieuTri}";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Lỗi đồng thời trong quá trình cập nhật. Có người đã cập nhật dữ liệu này trước bạn.";
                        return RedirectToAction(nameof(Edit), new { id = hinhThucDieuTri.MaDieuTri });
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Lỗi khi cập nhật hình thức điều trị: {ex.Message}";
                    _logger.LogError(ex, "Lỗi khi cập nhật hình thức điều trị ID {Id}", id);
                }
            }
            else
            {
                // Collect validation errors for display
                var validationErrors = new StringBuilder();
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        validationErrors.AppendLine($"• {error.ErrorMessage}");
                    }
                }
                TempData["ValidationErrors"] = validationErrors.ToString();
                
                _logger.LogWarning("Lỗi xác thực biểu mẫu khi cập nhật hình thức điều trị: {Errors}", 
                    string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)));
            }

            ViewData["MaKhoa"] = new SelectList(_context.Khoas, "MaKhoa", "TenKhoa", hinhThucDieuTri.MaKhoa);
            return View(hinhThucDieuTri);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID hình thức điều trị không được cung cấp";
                return RedirectToAction(nameof(Index));
            }

            var hinhThucDieuTri = await _context.HinhThucDieuTris
                .Include(h => h.Khoa)
                .FirstOrDefaultAsync(m => m.MaDieuTri == id);
                
            if (hinhThucDieuTri == null)
            {
                TempData["ErrorMessage"] = $"Không tìm thấy hình thức điều trị có ID: {id}";
                return RedirectToAction(nameof(Index));
            }

            // Check if this treatment type is being used
            var isInUse = await _context.DieuTriBenhNhans.AnyAsync(d => d.MaDieuTri == id);
            
            if (isInUse)
            {
                TempData["ErrorMessage"] = $"Không thể xóa hình thức điều trị '{hinhThucDieuTri.TenDieuTri}' vì đang được sử dụng trong các bản ghi điều trị.";
                return RedirectToAction(nameof(Index));
            }

            return View(hinhThucDieuTri);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var hinhThucDieuTri = await _context.HinhThucDieuTris
                    .Include(h => h.Khoa)
                    .FirstOrDefaultAsync(h => h.MaDieuTri == id);
                
                if (hinhThucDieuTri == null)
                {
                    TempData["ErrorMessage"] = $"Không tìm thấy hình thức điều trị có ID: {id}";
                    return RedirectToAction(nameof(Index));
                }

                // Check if this treatment type is being used
                var isInUse = await _context.DieuTriBenhNhans.AnyAsync(d => d.MaDieuTri == id);
                
                if (isInUse)
                {
                    TempData["ErrorMessage"] = $"Không thể xóa hình thức điều trị '{hinhThucDieuTri.TenDieuTri}' vì đang được sử dụng trong các bản ghi điều trị.";
                    return RedirectToAction(nameof(Index));
                }

                _context.HinhThucDieuTris.Remove(hinhThucDieuTri);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = $"Đã xóa thành công hình thức điều trị: {hinhThucDieuTri.TenDieuTri}";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa hình thức điều trị ID {Id}", id);
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xóa hình thức điều trị. Vui lòng thử lại sau.";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool HinhThucDieuTriExists(int id)
        {
            return _context.HinhThucDieuTris.Any(e => e.MaDieuTri == id);
        }

        // Endpoint để kiểm tra kết nối cơ sở dữ liệu và tên bảng
        [HttpGet("Diagnostic")]
        [AllowAnonymous]
        public IActionResult Diagnostic()
        {
            try
            {
                var model = new Dictionary<string, object>();
                
                // Kiểm tra kết nối
                bool canConnect = _context.Database.CanConnect();
                model.Add("DatabaseConnection", canConnect);
                
                // Lấy tên bảng HinhThucDieuTri
                var entityType = _context.Model.FindEntityType(typeof(HinhThucDieuTri));
                string tableName = entityType.GetTableName();
                string schema = entityType.GetSchema();
                model.Add("TableName", tableName);
                model.Add("Schema", schema ?? "(default)");
                
                // Kiểm tra có bản ghi trong bảng không
                int count = _context.HinhThucDieuTris.Count();
                model.Add("RecordCount", count);
                
                // Lấy danh sách các bảng trong CSDL
                if (canConnect) 
                {
                    var conn = _context.Database.GetDbConnection();
                    var tables = new List<string>();
                    
                    if (conn.State != System.Data.ConnectionState.Open)
                        conn.Open();
                    
                    // Lấy danh sách bảng từ SQL Server
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                            SELECT TABLE_SCHEMA, TABLE_NAME
                            FROM INFORMATION_SCHEMA.TABLES
                            WHERE TABLE_TYPE = 'BASE TABLE'
                            ORDER BY TABLE_SCHEMA, TABLE_NAME";
                        
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string tableSchema = reader.GetString(0);
                                string tableNameFromDb = reader.GetString(1);
                                tables.Add($"{tableSchema}.{tableNameFromDb}");
                            }
                        }
                    }
                    
                    model.Add("AllTables", tables);
                }
                
                // Lấy các bản ghi HinhThucDieuTri như JSON
                var treatments = _context.HinhThucDieuTris
                    .Include(h => h.Khoa)
                    .Take(10)
                    .Select(h => new { 
                        h.MaDieuTri, 
                        h.TenDieuTri, 
                        h.MaKhoa, 
                        KhoaTen = h.Khoa != null ? h.Khoa.TenKhoa : "(không có)",
                        h.ChiPhi, 
                        h.MoTa 
                    })
                    .ToList();
                
                model.Add("SampleRecords", treatments);
                
                // Thử thực hiện truy vấn EF Core raw
                try {
                    var efcoreResults = _context.HinhThucDieuTris
                        .FromSqlRaw($"SELECT * FROM {(string.IsNullOrEmpty(schema) ? "" : $"[{schema}].")}[{tableName}]")
                        .Take(5)
                        .ToList();
                    
                    model.Add("RawQueryRecordCount", efcoreResults.Count);
                } catch (Exception ex) {
                    model.Add("RawQueryError", ex.Message);
                }
                
                // Trả về kết quả dưới dạng JSON và giao diện HTML
                if (Request.Headers["Accept"].ToString().Contains("application/json"))
                {
                    return Json(model);
                }
                
                // Trả về view với dữ liệu chẩn đoán
                ViewBag.DiagnosticData = model;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi chạy chẩn đoán");
                return Json(new { 
                    Error = ex.Message,
                    StackTrace = ex.StackTrace,
                    InnerException = ex.InnerException?.Message
                });
            }
        }
    }
}