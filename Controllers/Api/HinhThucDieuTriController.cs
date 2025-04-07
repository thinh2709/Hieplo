using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBenhVienNoiTru.Data;
using QuanLyBenhVienNoiTru.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyBenhVienNoiTru.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class HinhThucDieuTriController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HinhThucDieuTriController> _logger;

        public HinhThucDieuTriController(ApplicationDbContext context, ILogger<HinhThucDieuTriController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/HinhThucDieuTri
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAll()
        {
            try
            {
                _logger.LogInformation("API Call: HinhThucDieuTri/GetAll");
                var result = await _context.HinhThucDieuTris
                    .Select(h => new {
                        maDieuTri = h.MaDieuTri,
                        tenDieuTri = h.TenDieuTri,
                        maKhoa = h.MaKhoa,
                        chiPhi = h.ChiPhi,
                        moTa = h.MoTa
                    })
                    .ToListAsync();
                
                _logger.LogInformation($"GetAll returned {result.Count} treatments");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetAll: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // GET: api/HinhThucDieuTri/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetById(int id)
        {
            try
            {
                _logger.LogInformation($"API Call: HinhThucDieuTri/GetById with id={id}");
                var item = await _context.HinhThucDieuTris
                    .Where(h => h.MaDieuTri == id)
                    .Select(h => new {
                        maDieuTri = h.MaDieuTri,
                        tenDieuTri = h.TenDieuTri,
                        maKhoa = h.MaKhoa,
                        chiPhi = h.ChiPhi,
                        moTa = h.MoTa
                    })
                    .FirstOrDefaultAsync();

                if (item == null)
                {
                    _logger.LogWarning($"GetById: Treatment with id={id} not found");
                    return NotFound();
                }

                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetById: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // GET: api/HinhThucDieuTri/ByKhoa/5
        [HttpGet("ByKhoa/{khoaId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetByKhoa(int khoaId)
        {
            try
            {
                _logger.LogInformation($"API Call: HinhThucDieuTri/GetByKhoa with khoaId={khoaId}");
                
                var result = await _context.HinhThucDieuTris
                    .Where(h => h.MaKhoa == khoaId)
                    .Select(h => new {
                        maDieuTri = h.MaDieuTri,
                        tenDieuTri = h.TenDieuTri,
                        maKhoa = h.MaKhoa,
                        chiPhi = h.ChiPhi,
                        moTa = h.MoTa
                    })
                    .ToListAsync();

                _logger.LogInformation($"GetByKhoa returned {result.Count} treatments for department id={khoaId}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetByKhoa: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
} 