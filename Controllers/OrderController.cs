using FoodOrderingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace FoodOrderingSystem.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult AddToCart(int foodItemId, int quantity = 1)
        {
            var cart = GetCart();

            var existingItem = cart.FirstOrDefault(
                c => c.FoodItemId == foodItemId
            );

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                var foodItem = _context.FoodItems
                    .FirstOrDefault(f => f.Id == foodItemId);

                if (foodItem == null)
                {
                    return NotFound("Food item not found.");
                }

                var cartItem = new CartItem
                {
                    FoodItemId = foodItem.Id,
                    Name = foodItem.Name,
                    Price = foodItem.Price,
                    Quantity = quantity,
                    ImageUrl = foodItem.ImageUrl
                };

                cart.Add(cartItem);
            }

            SaveCart(cart);

            return RedirectToAction("Cart");
        }
        // Get Cart from Session
        private List<CartItem> GetCart()
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            return cartJson == null ? new List<CartItem>() :
                 JsonConvert.DeserializeObject<List<CartItem>>(cartJson);
        }


        // Save Cart into Session
        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetString(
                "Cart",
                JsonConvert.SerializeObject(cart)
            );
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int foodItemId, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.FoodItemId == foodItemId);
            if (item != null && quantity > 0)
            {
                item.Quantity = quantity;
                SaveCart(cart);
            }
            return RedirectToAction("Cart");
        }


        public IActionResult RemoveFormCart(int foodItemId)
        {
            var cart = GetCart();
            var cartItem = cart.FirstOrDefault(c => c.FoodItemId == foodItemId);
            if (cartItem != null)
            {
                cart.Remove(cartItem);
                SaveCart(cart);
            }
            return RedirectToAction("Cart");
        }
        // Cart Page
        public IActionResult Cart()
        {
            var cart = GetCart();
            ViewBag.Total = cart.Sum(c => c.Price * c.Quantity);
            return View(cart);
        }
  
        [HttpGet]
        public IActionResult Checkout()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");
            var cart = GetCart();
            if (!cart.Any())
                return RedirectToAction("Index", "Menu");
            ViewBag.Total = cart.Sum(c => c.Price * c.Quantity);
            return View();
        }

        [HttpPost]
        public IActionResult Checkout(string deliveryAddress, string phone)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var cart = GetCart();
            var order = new Order
            {
                UserId = userId.Value,
                OrderDate = DateTime.Now,
                DeliveryAddress = deliveryAddress,
                Phone = phone,
                TotalAmount = cart.Sum(c => c.Price * c.Quantity),
                OrderItems = cart.Select(c => new OrderItem
                {
                    FoodItemId = c.FoodItemId,
                    Quantity = c.Quantity,
                    UnitPrice = c.Price
                }).ToList()
            };
            _context.Orders.Add(order);
            _context.SaveChanges();
            HttpContext.Session.Remove("Cart");
            return RedirectToAction("OrderConfirmation", new { orderId = order.Id });
        }

        public IActionResult OrderConfirmation(int orderId)
        {
            ViewBag.OrderId = orderId;
            return View();
        }

        public IActionResult MyOrders(string status, string sortOrder, DateTime? fromDate, DateTime? endDate)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");
            var orders = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.FoodItem)
                .Where(o => o.UserId == userId).AsQueryable()
                .OrderByDescending(o => o.OrderDate)
                .ToList();
            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                orders = orders.Where(o => o.Status == status).ToList();
                ViewBag.CurrentStatus = status;
            }

            if (fromDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate >= fromDate.Value).ToList();
                ViewBag.FromDate = fromDate.Value.ToString("yyyy-MM-dd");
            }

            if(endDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate <= endDate.Value.AddDays(1)).ToList();
                ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");
            }

            ViewBag.CurrentSort = sortOrder;
            orders = sortOrder switch
            {
                "date_asc" => orders.OrderBy(o => o.OrderDate).ToList(),
                "date_desc" => orders.OrderByDescending(o => o.OrderDate).ToList(),
                "total_desc" => orders.OrderByDescending(o => o.TotalAmount).ToList(),
                "total_asc" => orders.OrderBy(o => o.TotalAmount).ToList(),
                _=> orders.OrderByDescending(o => o.OrderDate).ToList(),

            };
            ViewBag.StatusList = new List<string> { "All", "Pending", "Completed", "Preparing", "OutForDelivery", "Cancelled" };
            return View(orders);


        }




        // CartItem is NOT a database table
        public class CartItem
        {
            public int FoodItemId { get; set; }

            public string? Name { get; set; }

            public decimal Price { get; set; }

            public int Quantity { get; set; }

            public string? ImageUrl { get; set; }
        }
    }
}