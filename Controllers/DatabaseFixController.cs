using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBenhVienNoiTru.Data;
using QuanLyBenhVienNoiTru.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyBenhVienNoiTru.Controllers
{
    public class DatabaseFixController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DatabaseFixController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hiển thị trang chẩn đoán
        public IActionResult Index()
        {
            return View();
        }

        // API endpoint trả về danh sách các bảng
        [HttpGet("api/tables")]
        public IActionResult GetTables()
        {
            try
            {
                if (!_context.Database.CanConnect())
                {
                    return Json(new { error = "Không thể kết nối đến cơ sở dữ liệu" });
                }
                
                var conn = _context.Database.GetDbConnection();
                var tables = new List<object>();
                
                if (conn.State != System.Data.ConnectionState.Open)
                    conn.Open();
                
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT 
                            t.TABLE_SCHEMA, 
                            t.TABLE_NAME,
                            (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS c WHERE c.TABLE_SCHEMA = t.TABLE_SCHEMA AND c.TABLE_NAME = t.TABLE_NAME) AS ColumnCount
                        FROM 
                            INFORMATION_SCHEMA.TABLES t
                        WHERE 
                            t.TABLE_TYPE = 'BASE TABLE'
                        ORDER BY 
                            t.TABLE_SCHEMA, t.TABLE_NAME";
                    
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tables.Add(new { 
                                schema = reader.GetString(0),
                                name = reader.GetString(1),
                                fullName = $"{reader.GetString(0)}.{reader.GetString(1)}",
                                columnCount = reader.GetInt32(2)
                            });
                        }
                    }
                }
                
                // Kiểm tra xem bảng HinhThucDieuTris có tồn tại không
                bool hasHinhThucDieuTrisTable = tables.Any(t => 
                    ((dynamic)t).name.ToString().Equals("HinhThucDieuTris", StringComparison.OrdinalIgnoreCase));
                
                return Json(new { 
                    success = true, 
                    tables = tables,
                    hasHinhThucDieuTrisTable = hasHinhThucDieuTrisTable
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // Endpoint để tạo bảng HinhThucDieuTris nếu không tồn tại
        [HttpPost("api/create-table")]
        public async Task<IActionResult> CreateHinhThucDieuTrisTable()
        {
            try
            {
                if (!_context.Database.CanConnect())
                {
                    return Json(new { error = "Không thể kết nối đến cơ sở dữ liệu" });
                }
                
                var conn = _context.Database.GetDbConnection();
                
                if (conn.State != System.Data.ConnectionState.Open)
                    conn.Open();
                
                // Kiểm tra xem bảng đã tồn tại chưa
                bool tableExists = false;
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT COUNT(*) 
                        FROM INFORMATION_SCHEMA.TABLES 
                        WHERE TABLE_NAME = 'HinhThucDieuTris'";
                    
                    tableExists = (int)cmd.ExecuteScalar() > 0;
                }
                
                if (tableExists)
                {
                    return Json(new { 
                        success = true, 
                        message = "Bảng HinhThucDieuTris đã tồn tại." 
                    });
                }
                
                // Tạo bảng HinhThucDieuTris
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        CREATE TABLE [dbo].[HinhThucDieuTris](
                            [MaDieuTri] [int] IDENTITY(1,1) NOT NULL,
                            [TenDieuTri] [nvarchar](max) NOT NULL,
                            [MaKhoa] [int] NOT NULL,
                            [ChiPhi] [decimal](18, 2) NOT NULL,
                            [MoTa] [nvarchar](max) NOT NULL,
                            [BacSiMaBacSi] [int] NULL,
                            [BenhNhanMaBenhNhan] [int] NULL,
                            CONSTRAINT [PK_HinhThucDieuTris] PRIMARY KEY CLUSTERED ([MaDieuTri] ASC)
                        )";
                    
                    await cmd.ExecuteNonQueryAsync();
                }
                
                // Tạo khóa ngoại cho bảng
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        ALTER TABLE [dbo].[HinhThucDieuTris] 
                        ADD CONSTRAINT [FK_HinhThucDieuTris_Khoas_MaKhoa] 
                        FOREIGN KEY([MaKhoa]) REFERENCES [dbo].[Khoas] ([MaKhoa])";
                    
                    await cmd.ExecuteNonQueryAsync();
                }
                
                return Json(new { 
                    success = true, 
                    message = "Bảng HinhThucDieuTris đã được tạo thành công." 
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
        
        // Endpoint để thêm dữ liệu mẫu vào bảng HinhThucDieuTris
        [HttpPost("api/add-sample-data")]
        public async Task<IActionResult> AddSampleData()
        {
            try
            {
                // Kiểm tra xem đã có dữ liệu trong bảng chưa
                if (_context.HinhThucDieuTris.Any())
                {
                    return Json(new { 
                        success = true, 
                        message = "Bảng HinhThucDieuTris đã có dữ liệu." 
                    });
                }
                
                // Lấy danh sách khoa
                var khoas = await _context.Khoas.ToListAsync();
                if (!khoas.Any())
                {
                    return Json(new { 
                        error = "Không có dữ liệu khoa. Vui lòng thêm khoa trước." 
                    });
                }
                
                // Danh sách dữ liệu mẫu
                var sampleTreatments = new List<HinhThucDieuTri>();
                
                foreach (var khoa in khoas)
                {
                    // Thêm các dữ liệu mẫu cho từng khoa
                    sampleTreatments.Add(new HinhThucDieuTri
                    {
                        TenDieuTri = "Siêu âm tổng quát",
                        MaKhoa = khoa.MaKhoa,
                        ChiPhi = 250000,
                        MoTa = "Siêu âm tổng quát để chẩn đoán"
                    });
                    
                    sampleTreatments.Add(new HinhThucDieuTri
                    {
                        TenDieuTri = "Xét nghiệm máu",
                        MaKhoa = khoa.MaKhoa,
                        ChiPhi = 100000,
                        MoTa = "Xét nghiệm máu cơ bản"
                    });
                    
                    sampleTreatments.Add(new HinhThucDieuTri
                    {
                        TenDieuTri = "Chụp X-quang",
                        MaKhoa = khoa.MaKhoa,
                        ChiPhi = 200000,
                        MoTa = "Chụp X-quang tổng quát"
                    });
                }
                
                // Thêm dữ liệu vào database
                await _context.HinhThucDieuTris.AddRangeAsync(sampleTreatments);
                await _context.SaveChangesAsync();
                
                return Json(new { 
                    success = true,
                    message = $"Đã thêm {sampleTreatments.Count} dữ liệu mẫu." 
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
    }
} 