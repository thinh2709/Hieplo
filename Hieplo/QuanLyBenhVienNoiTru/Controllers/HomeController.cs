using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBenhVienNoiTru.Data;
using QuanLyBenhVienNoiTru.Models;

namespace QuanLyBenhVienNoiTru.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Lấy thống kê cho trang chủ
        ViewBag.BenhNhanCount = await _context.BenhNhans.CountAsync(b => b.NgayXuatVien == null);
        ViewBag.BacSiCount = await _context.BacSis.CountAsync(b => b.TrangThai == true);
        ViewBag.KhoaCount = await _context.Khoas.CountAsync();
        ViewBag.LichThamBenhCount = await _context.LichThamBenhs
            .CountAsync(l => l.ThoiGianTham.HasValue && l.ThoiGianTham.Value.Date == DateTime.Today);
            
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Help()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
