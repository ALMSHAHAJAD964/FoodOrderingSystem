using FoodOrderingSystem.Models;
using FoodOrderingSystem.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderingSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    Username = model.Username,
                    Email = model.Email,
                    Password = model.Password,
                    FullName = model.FullName,
                    Address = model.Address,
                    Phone = model.Phone


                };
                _context.Users.Add(user);
                _context.SaveChanges();
                return RedirectToAction("Login");


            }
            return View(model);

        }

        public IActionResult Login(LoginViewModels loginViewModels)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == loginViewModels.Username && u.Password == loginViewModels.Password);
            if (user != null)
            {
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("IsAdmin", user.IsAdmin.ToString());
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("", "Invalid username or password");
            }
            return View(loginViewModels);
        }

        [HttpGet]
        public IActionResult Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)

                return RedirectToAction("Login");

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return NotFound("User not found.");
            var model = new ProfileViewModel
            {
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Address = user.Address,
                Phone = user.Phone
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Profile(ProfileViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");
            if (!ModelState.IsValid)
            {
                var user = _context.Users.Find(userId);
                var emailExists = _context.Users.Any(u => u.Email == model.Email && u.Id != userId);
                if (emailExists)
                {
                    ModelState.AddModelError("Email", "Email is already registered to another account.");
                    return View(model);
                }
                user.FullName = model.FullName;
                user.Phone = model.Phone;
                user.Email = model.Email;
                user.Address = model.Address;
                _context.SaveChanges();
                TempData["Success"] = "Profile updated successfully.";
                return RedirectToAction("Profile");
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult DeleteAccount()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login");

            var user = _context.Users.Find(userId);
            if (user == null)
                return NotFound("User not found.");

            var pendingOrders = _context.Orders.Any(o => o.UserId == userId &&
            (o.Status == "Pending" || o.Status == "Confirmed" || o.Status == "Processing"));
            if (pendingOrders)
            {
                TempData["Error"] = "You cannot delete your account while you have pending orders.";
                return RedirectToAction("Profile");
            }
            _context.Users.Remove(user);
            _context.SaveChanges();
            HttpContext.Session.Clear();
            TempData["Success"] = "Your account has been deleted successfully.";
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");

        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login");
            if (!ModelState.IsValid)
                return View(model);
            var user = _context.Users.Find(userId);
            if (user == null)
                return NotFound("User not found.");
            if (user.Password != model.CurrentPassword)
            {
                ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                return View(model);
            }
            user.Password = model.NewPassword;
            _context.SaveChanges();
            TempData["Success"] = "Password changed successfully.";
            return RedirectToAction("Profile");
        }
    }
}
