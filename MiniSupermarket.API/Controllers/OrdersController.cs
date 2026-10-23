using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniSupermarket.API.Data;
using MiniSupermarket.API.Models;

namespace MiniSupermarket.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly SupermarketDbContext _context;

        public OrdersController(SupermarketDbContext context)
        {
            _context = context;
        }

        // 1. GET: api/orders (Lấy danh sách hóa đơn)
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .AsNoTracking()
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return Ok(orders);
        }

        // 2. POST: api/orders/checkout (Thanh toán đơn hàng POS - Dùng Transaction)
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequestDto request)
        {
            if (request == null || request.Items == null || !request.Items.Any())
            {
                return BadRequest(new { message = "Giỏ hàng trống, không thể thanh toán." });
            }

            // Tìm UserId từ CashierUsername
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.CashierUsername);
            if (user == null)
            {
                return BadRequest(new { message = "Không tìm thấy thông tin nhân viên thu ngân." });
            }

            // Tìm CustomerId từ CustomerPhone (nếu có nhập SĐT)
            int? customerId = null;
            if (!string.IsNullOrWhiteSpace(request.CustomerPhone))
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.PhoneNumber == request.CustomerPhone);
                if (customer != null)
                {
                    customerId = customer.CustomerId;
                    customer.RewardPoints += 10; // Tự động cộng điểm tích lũy khi mua hàng
                }
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                decimal totalAmount = 0;
                var order = new Order
                {
                    UserId = user.Id, // Đã sửa từ user.UserId thành user.Id cho khớp với Model User
                    CustomerId = customerId,
                    OrderDate = DateTime.Now,
                    PaymentMethod = request.PaymentMethod ?? "Tiền mặt",
                    TotalAmount = 0
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                foreach (var item in request.Items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product == null)
                    {
                        throw new Exception($"Không tìm thấy sản phẩm có ID: {item.ProductId}");
                    }

                    if (product.StockQuantity < item.Quantity)
                    {
                        throw new Exception($"Sản phẩm '{product.ProductName}' không đủ tồn kho (Còn lại: {product.StockQuantity}).");
                    }

                    product.StockQuantity -= item.Quantity;

                    decimal lineTotal = product.Price * item.Quantity;
                    totalAmount += lineTotal;

                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.OrderId,
                        ProductId = product.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price
                    };

                    _context.OrderDetails.Add(orderDetail);
                }

                order.TotalAmount = totalAmount;
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Thanh toán thành công!",
                    orderId = order.OrderId,
                    totalAmount = totalAmount
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { message = "Thanh toán thất bại: " + ex.Message });
            }
        }
    }

    // DTO khớp hoàn toàn với dữ liệu FormPOS gửi lên
    public class CheckoutRequestDto
    {
        public string CashierUsername { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        public string PaymentMethod { get; set; } = "Tiền mặt";
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
    }

    public class CartItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}