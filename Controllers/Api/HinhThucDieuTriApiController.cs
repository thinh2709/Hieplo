using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBenhVienNoiTru.Data;
using QuanLyBenhVienNoiTru.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace QuanLyBenhVienNoiTru.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class HinhThucDieuTriApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HinhThucDieuTriApiController> _logger;

        public HinhThucDieuTriApiController(ApplicationDbContext context, ILogger<HinhThucDieuTriApiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/HinhThucDieuTriApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HinhThucDieuTri>>> GetAllHinhThucDieuTri()
        {
            try
            {
                _logger.LogInformation("API Call: GetAllHinhThucDieuTri");
                var result = await _context.HinhThucDieuTris
                    .Select(h => new {
                        maDieuTri = h.MaDieuTri,
                        tenDieuTri = h.TenDieuTri,
                        maKhoa = h.MaKhoa,
                        chiPhi = h.ChiPhi,
                        moTa = h.MoTa
                    })
                    .ToListAsync();
                
                _logger.LogInformation($"GetAllHinhThucDieuTri returned {result.Count} treatments");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetAllHinhThucDieuTri: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // GET: api/HinhThucDieuTriApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<HinhThucDieuTri>> GetHinhThucDieuTri(int id)
        {
            try
            {
                _logger.LogInformation($"API Call: GetHinhThucDieuTri with id={id}");
                var hinhThucDieuTri = await _context.HinhThucDieuTris
                    .Include(h => h.Khoa)
                    .FirstOrDefaultAsync(h => h.MaDieuTri == id);

                if (hinhThucDieuTri == null)
                {
                    _logger.LogWarning($"GetHinhThucDieuTri: Treatment with id={id} not found");
                    return NotFound();
                }

                _logger.LogInformation($"GetHinhThucDieuTri returned: {hinhThucDieuTri.TenDieuTri}");
                return hinhThucDieuTri;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetHinhThucDieuTri: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // GET: api/HinhThucDieuTriApi/ByKhoa/5
        [HttpGet("ByKhoa/{khoaId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetHinhThucDieuTriByKhoa(int khoaId)
        {
            try
            {
                _logger.LogInformation($"API Call: GetHinhThucDieuTriByKhoa with khoaId={khoaId}");
                
                // Truy vấn hình thức điều trị theo khoa và chuyển đổi về dạng đơn giản hơn
                var hinhThucDieuTriList = await _context.HinhThucDieuTris
                    .Where(h => h.MaKhoa == khoaId)
                    .Select(h => new {
                        maDieuTri = h.MaDieuTri,
                        tenDieuTri = h.TenDieuTri,
                        maKhoa = h.MaKhoa,
                        chiPhi = h.ChiPhi,
                        moTa = h.MoTa
                    })
                    .ToListAsync();

                _logger.LogInformation($"GetHinhThucDieuTriByKhoa returned {hinhThucDieuTriList.Count} treatments for department id={khoaId}");
                
                return Ok(hinhThucDieuTriList);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetHinhThucDieuTriByKhoa: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // GET: api/HinhThucDieuTriApi/test
        [HttpGet("test")]
        public ActionResult<object> TestApi()
        {
            try
            {
                _logger.LogInformation("API Test endpoint called");
                return new { 
                    status = "success", 
                    message = "API is working correctly!",
                    timestamp = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Test API error: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // GET: api/HinhThucDieuTriApi/test-treatments/{khoaId}
        [HttpGet("test-treatments/{khoaId}")]
        public async Task<ActionResult<object>> TestTreatments(int khoaId)
        {
            try
            {
                _logger.LogInformation($"Test-treatments API called for khoaId={khoaId}");
                
                // Tạo dữ liệu mẫu cho việc kiểm tra
                var mockTreatments = new List<object>
                {
                    new { maDieuTri = khoaId * 1000 + 1, tenDieuTri = "Siêu âm tổng quát", maKhoa = khoaId, chiPhi = 250000m, moTa = "Siêu âm tổng quát để chẩn đoán" },
                    new { maDieuTri = khoaId * 1000 + 2, tenDieuTri = "Xét nghiệm máu", maKhoa = khoaId, chiPhi = 100000m, moTa = "Xét nghiệm máu cơ bản" },
                    new { maDieuTri = khoaId * 1000 + 3, tenDieuTri = "Chụp X-quang", maKhoa = khoaId, chiPhi = 200000m, moTa = "Chụp X-quang tổng quát" }
                };
                
                // Cố gắng lấy dữ liệu thật từ database
                var realTreatments = await _context.HinhThucDieuTris
                    .Where(h => h.MaKhoa == khoaId)
                    .Select(h => new { 
                        maDieuTri = h.MaDieuTri, 
                        tenDieuTri = h.TenDieuTri, 
                        maKhoa = h.MaKhoa, 
                        chiPhi = h.ChiPhi, 
                        moTa = h.MoTa 
                    })
                    .ToListAsync();
                
                return new { 
                    status = "success", 
                    mockData = mockTreatments,
                    realData = realTreatments,
                    count = realTreatments.Count,
                    message = realTreatments.Count > 0 ? "Found real treatments" : "No real treatments found"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Test treatments API error: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // GET: api/HinhThucDieuTriApi/debug
        [HttpGet("debug")]
        public ActionResult<object> DebugDatabaseConnection()
        {
            _logger.LogInformation("API Call: DebugDatabaseConnection");
            
            try
            {
                // Kiểm tra kết nối
                bool canConnect = _context.Database.CanConnect();
                
                // Lấy tên bảng từ DbContext
                var entityType = _context.Model.FindEntityType(typeof(HinhThucDieuTri));
                string tableName = entityType.GetTableName();
                string schema = entityType.GetSchema();
                
                // Đếm số bản ghi
                int totalRecords = _context.HinhThucDieuTris.Count();
                
                // Lấy dữ liệu mẫu
                var sampleData = _context.HinhThucDieuTris
                    .Take(5)
                    .Select(x => new { 
                        x.MaDieuTri, 
                        x.TenDieuTri, 
                        x.MaKhoa, 
                        x.ChiPhi, 
                        x.MoTa 
                    })
                    .ToList();
                
                return new 
                { 
                    status = "success", 
                    databaseConnection = canConnect,
                    tableName = tableName,
                    schema = schema ?? "(default)",
                    totalRecords = totalRecords,
                    sampleData = sampleData,
                    timestamp = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Debug database connection error: {ex.Message}");
                return StatusCode(500, new { 
                    status = "error", 
                    message = ex.Message,
                    stackTrace = ex.StackTrace,
                    innerException = ex.InnerException?.Message
                });
            }
        }
    }
} 