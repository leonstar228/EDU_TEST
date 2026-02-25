using EDU_TEST.Data;
using EDU_TEST.Models;
using EDU_TEST.ViewModels;
using EDU_TEST.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EDU_TEST.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class SuperAdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher _hasher;

        public SuperAdminController(ApplicationDbContext context, PasswordHasher hasher)
        {
            _context = context;
            _hasher = hasher;
        }
        
        public IActionResult Index()
        {
            return View();
        }
        
        public IActionResult CreateAdmin()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateAdmin(CreateAdminViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (_context.Users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("", "Користувач з таким email вже існує");
                return View(model);
            }

            var admin = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PasswordHash = _hasher.Hash(model.Password),
                Role = "Admin",
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(admin);
            _context.SaveChanges();

            return RedirectToAction("AdminList");
        }
        
        public IActionResult AdminList()
        {
            var admins = _context.Users
                .Where(u => u.Role == "Admin")
                .ToList();

            return View(admins);
        }
    }
}