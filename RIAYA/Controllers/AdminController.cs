using Microsoft.AspNetCore.Mvc;
using RIAYA.Models;

namespace RIAYA.Controllers
{
    public class AdminController : Controller
    {
        private readonly MyDbContext _context;

        public AdminController(MyDbContext context)
        {
            _context = context;
        }

        public IActionResult AdminDashboard()
        {
            return View();
        }

        public IActionResult Providers()
        {
            return View();
        }

        public IActionResult Users()
        {
            return View();
        }
        public IActionResult Categories()
        {
            return View();
        }
        public IActionResult Reservations()
        {
            return View();
        }
        public IActionResult ContactUs()
        {
            return View();
        }
        public IActionResult HealthBlogs()
        {
            return View();
        }

    }
}
