using FoodOrderingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderingSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }
        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("IsAdmin") == "True";
        }

        public IActionResult Dashboard()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.TotalOrders = _context.Orders.Count();
            ViewBag.TotalUsers = _context.Users.Count();
            ViewBag.TotalItems = _context.FoodItems.Count();

            return View();
        }

        public IActionResult ManageMenu(int? categoryId, string searchString, bool? isAvailable, decimal? minPrice, decimal? maxPrice,
            string sortOrder)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParam = sortOrder == "name_desc" ? "name_asc" : "name_desc";
            ViewBag.PriceSortParam = sortOrder == "price_asc" ? "price_desc" : "price_asc";
            ViewBag.CategorySortParam = sortOrder == "category" ? "category_desc" : "category";

            var items = _context.FoodItems.Include(f => f.Category).AsQueryable();

            if (categoryId.HasValue && categoryId > 0)
            {
                items = items.Where(f => f.CategoryId == categoryId.Value);
                ViewBag.CurrentCategory = categoryId;
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                items = items.Where(f => f.Name.Contains(searchString) || f.Description.Contains(searchString));
                ViewBag.CurrentSearch = searchString;
            }


            if (isAvailable.HasValue)
            {
                items = items.Where(f => f.IsAvailable == isAvailable.Value);
                ViewBag.CurrentAvailability = isAvailable;
            }

            if (minPrice.HasValue)
            {
                items = items.Where(f => f.Price >= minPrice.Value);
                ViewBag.CurrentMinPrice = minPrice;
            }

            if (maxPrice.HasValue)
            {
                items = items.Where(f => f.Price <= maxPrice.Value);
                ViewBag.CurrentMaxPrice = maxPrice;
            }

            items = sortOrder switch
            {
                "name_desc" => items.OrderByDescending(f => f.Name),
                "name_asc" => items.OrderBy(f => f.Name),
                "price_desc" => items.OrderByDescending(f => f.Price),
                "price_asc" => items.OrderBy(f => f.Price),
                "category" => items.OrderBy(f => f.Category.Name),
                "category_desc" => items.OrderByDescending(f => f.Category.Name),
                _ => items.OrderBy(f => f.CategoryId).ThenBy(f => f.Name),
            };

            ViewBag.TotalItems = _context.FoodItems.Count();
            ViewBag.AvailableItems = _context.FoodItems.Count(f => f.IsAvailable);
            ViewBag.UnavailableItems = _context.FoodItems.Count(f => !f.IsAvailable);
            ViewBag.CategoryCount = _context.Categories.Count();
            ViewBag.Categories = _context.Categories.ToList();
            return View(items.ToList());
        }
        [HttpPost]
        public IActionResult ToggleAvailability(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }
            var item = _context.FoodItems.Find(id);
            if (item == null)
            {
                return NotFound();
            }
            item.IsAvailable = !item.IsAvailable;
            _context.SaveChanges();
            TempData["Success"] = $"{item.Name} is now {(item.IsAvailable ? "available" : "unavailable")}";
            return RedirectToAction("ManageMenu");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }
            var item = _context.FoodItems.Find(id);
            if (item == null)
            {
                return NotFound();
            }
            _context.FoodItems.Remove(item);
            _context.SaveChanges();
            TempData["Success"] = $"{item.Name} has been deleted";
            return RedirectToAction("ManageMenu");
        }

        [HttpGet]
        public IActionResult AddFoodItem()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult AddFoodItem(FoodItem item)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }
            _context.FoodItems.Add(item);
            _context.SaveChanges();
            return RedirectToAction("ManageMenu");
        }

        [HttpGet]
        public IActionResult EditFoodItem( int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }
            var item = _context.FoodItems.Find(id);
            if(item == null)
            {
                return NotFound();
            }
            ViewBag.Categories = _context.Categories.ToList();
            return View(item);
        }

        [HttpPost]
        public IActionResult EditFoodItem([Bind("Id,Name,Description,Price,CategoryId,ImageUrl,IsAvailable")] FoodItem item)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }
            if (ModelState.IsValid)
            {
                var existingItem = _context.FoodItems.Find(item.Id);
                if(existingItem == null)
                {
                    return NotFound();
                }
                existingItem.Name = item.Name;
                existingItem.Description = item.Description;
                existingItem.Price = item.Price;
                existingItem.CategoryId = item.CategoryId;
                existingItem.ImageUrl = item.ImageUrl;
                existingItem.IsAvailable = item.IsAvailable;
                _context.SaveChanges();
                TempData["Success"] =$"{item.Name} has been updated successfully";
                return RedirectToAction("ManageMenu");
            }
            ViewBag.Categories = _context.Categories.ToList();
            return View(item) ;
        }
    }
}
