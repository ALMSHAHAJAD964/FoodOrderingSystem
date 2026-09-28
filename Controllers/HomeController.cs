using FoodOrderingSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderingSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext context;

        public HomeController(ApplicationDbContext context)
        {
            this.context = context;
        }
        public IActionResult Index()
        {
            var foodItems = context.FoodItems.Where(f => f.IsAvailable).ToList();
            return View(foodItems);
        }
    }
}
