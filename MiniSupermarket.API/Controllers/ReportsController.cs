
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniSupermarket.API.Data;

namespace MiniSupermarket.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly SupermarketDbContext _context;

        public ReportsController(SupermarketDbContext context)
        {
            _context = context;
        }

        // GET: api/reports/daily?date=2026-10-22
        [HttpGet("daily")]
        public async Task<IActionResult> GetDailyReport(
            [FromQuery] DateTime? date)
        {
            try
            {
                DateTime targetDate = date ?? DateTime.Today;
                DateTime startDate = targetDate.Date;
                DateTime endDate = startDate.AddDays(1);

                // 1. Lay danh sach hoa don trong ngay
                var orders = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                    .Where(o => o.OrderDate >= startDate &&
                                o.OrderDate < endDate)
                    .ToListAsync();

                // 2. Tong so hoa don trong ngay
                int totalOrders = orders.Count;

                // 3. Tong doanh thu trong ngay
                decimal totalRevenue = orders.Sum(o => o.TotalAmount);

                // 4. Tim san pham ban chay nhat trong ngay
                var bestSellingProduct = orders
                    .SelectMany(o => o.OrderDetails)
                    .Where(od => od.Product != null &&
                                 !string.IsNullOrEmpty(
                                     od.Product.ProductName))
                    .GroupBy(od => od.Product!.ProductName)
                    .Select(g => new
                    {
                        ProductName = g.Key,
                        TotalQty = g.Sum(x => x.Quantity)
                    })
                    .OrderByDescending(x => x.TotalQty)
                    .FirstOrDefault()?.ProductName
                    ?? "Chua co du lieu";

                return Ok(new
                {
                    TotalOrders = totalOrders,
                    TotalRevenue = totalRevenue,
                    BestSellingProduct = bestSellingProduct
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Loi server khi lay bao cao: " + ex.Message
                });
            }
        }

        // GET: api/reports/dashboard-summary
        [HttpGet("dashboard-summary")]
        public async Task<IActionResult> GetDashboardSummary()
        {
            try
            {
                DateTime today = DateTime.Today;
                DateTime tomorrow = today.AddDays(1);

                // 1. Doanh thu hom nay
                decimal todayRevenue = await _context.Orders
                    .Where(o => o.OrderDate >= today &&
                                o.OrderDate < tomorrow)
                    .Select(o => (decimal?)o.TotalAmount)
                    .SumAsync() ?? 0m;

                // 2. Tong so hoa don
                int totalOrders = await _context.Orders
                    .CountAsync();

                // 3. So san pham sap het hang
                // Quy uoc: ton kho <= 10 la sap het hang
                int lowStockProducts = await _context.Products
                    .CountAsync(p => p.StockQuantity <= 10);

                return Ok(new
                {
                    TodayRevenue = todayRevenue,
                    TotalOrders = totalOrders,
                    LowStockProducts = lowStockProducts
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Loi khi lay Dashboard: " + ex.Message
                });
            }
        }
    }
}
