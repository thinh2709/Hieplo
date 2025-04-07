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
    public class BacSiApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BacSiApiController> _logger;

        public BacSiApiController(ApplicationDbContext context, ILogger<BacSiApiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/BacSiApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BacSi>>> GetAllBacSi()
        {
            _logger.LogInformation("API Call: GetAllBacSi");
            var result = await _context.BacSis.ToListAsync();
            _logger.LogInformation($"GetAllBacSi returned {result.Count} doctors");
            return result;
        }

        // GET: api/BacSiApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BacSi>> GetBacSi(int id)
        {
            _logger.LogInformation($"API Call: GetBacSi with id={id}");
            var bacSi = await _context.BacSis.FindAsync(id);

            if (bacSi == null)
            {
                _logger.LogWarning($"GetBacSi: Doctor with id={id} not found");
                return NotFound();
            }

            _logger.LogInformation($"GetBacSi returned: {bacSi.HoTen}");
            return bacSi;
        }

        // GET: api/BacSiApi/ByKhoa/5
        [HttpGet("ByKhoa/{khoaId}")]
        public async Task<ActionResult<IEnumerable<BacSi>>> GetBacSiByKhoa(int khoaId)
        {
            _logger.LogInformation($"API Call: GetBacSiByKhoa with khoaId={khoaId}");
            
            try
            {
                var bacSiList = await _context.BacSis
                    .Where(b => b.MaKhoa == khoaId && b.TrangThai)
                    .ToListAsync();

                _logger.LogInformation($"GetBacSiByKhoa returned {bacSiList.Count} doctors for department id={khoaId}");
                
                // Log each doctor found for debugging
                foreach (var bacSi in bacSiList)
                {
                    _logger.LogInformation($"Doctor: ID={bacSi.MaBacSi}, Name={bacSi.HoTen}, Department={bacSi.MaKhoa}");
                }
                
                return bacSiList;
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetBacSiByKhoa error: {ex.Message}");
                throw;
            }
        }
    }
} 