using EDU_TEST.Data;
using EDU_TEST.Models;
using EDU_TEST.ViewModels;
using EDU_TEST.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EDU_TEST.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher _hasher;

        public AuthController(ApplicationDbContext context, PasswordHasher hasher)
        {
            _context = context;
            _hasher = hasher;
        }
        
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "Паролі не співпадають");
                return View(model);
            }

            if (_context.Users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("", "Користувач з таким email вже існує");
                return View(model);
            }

            var user = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PasswordHash = _hasher.Hash(model.Password),
                Role = "Student",
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }

       
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "Користувача не знайдено");
                return View(model);
            }

            if (user.PasswordHash != _hasher.Hash(model.Password))
            {
                ModelState.AddModelError("", "Невірний пароль");
                return View(model);
            }

            // Claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("UserId", user.Id.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);


            if (user.Role == "Admin") return RedirectToAction("Index", "Admin");
            
            return RedirectToAction("Index", "Student");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}