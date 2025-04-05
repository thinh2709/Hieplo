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
    public class HinhThucDieuTriApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HinhThucDieuTriApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/HinhThucDieuTriApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HinhThucDieuTri>>> GetAllHinhThucDieuTri()
        {
            return await _context.HinhThucDieuTris.ToListAsync();
        }

        // GET: api/HinhThucDieuTriApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<HinhThucDieuTri>> GetHinhThucDieuTri(int id)
        {
            var hinhThucDieuTri = await _context.HinhThucDieuTris.FindAsync(id);

            if (hinhThucDieuTri == null)
            {
                return NotFound();
            }

            return hinhThucDieuTri;
        }

        // GET: api/HinhThucDieuTriApi/ByKhoa/5
        [HttpGet("ByKhoa/{khoaId}")]
        public async Task<ActionResult<IEnumerable<HinhThucDieuTri>>> GetHinhThucDieuTriByKhoa(int khoaId)
        {
            // Truy vấn hình thức điều trị theo khoa
            var hinhThucDieuTriList = await _context.HinhThucDieuTris
                .Where(h => h.MaKhoa == khoaId)
                .ToListAsync();

            return hinhThucDieuTriList;
        }

        // Trả về thông tin chi phí của tất cả các hình thức điều trị
        [HttpGet("Costs")]
        public async Task<ActionResult<Dictionary<int, decimal>>> GetAllCosts()
        {
            var costs = await _context.HinhThucDieuTris
                .ToDictionaryAsync(h => h.MaDieuTri, h => h.ChiPhi);
                
            return costs;
        }
    }
} 