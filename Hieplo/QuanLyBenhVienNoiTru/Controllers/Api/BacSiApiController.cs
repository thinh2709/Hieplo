using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBenhVienNoiTru.Data;
using QuanLyBenhVienNoiTru.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyBenhVienNoiTru.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class BacSiApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BacSiApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/BacSiApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BacSi>>> GetAllBacSi()
        {
            return await _context.BacSis.ToListAsync();
        }

        // GET: api/BacSiApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BacSi>> GetBacSi(int id)
        {
            var bacSi = await _context.BacSis.FindAsync(id);

            if (bacSi == null)
            {
                return NotFound();
            }

            return bacSi;
        }

        // GET: api/BacSiApi/ByKhoa/5
        [HttpGet("ByKhoa/{khoaId}")]
        public async Task<ActionResult<IEnumerable<BacSi>>> GetBacSiByKhoa(int khoaId)
        {
            var bacSiList = await _context.BacSis
                .Where(b => b.MaKhoa == khoaId)
                .ToListAsync();

            return bacSiList;
        }
    }
} 