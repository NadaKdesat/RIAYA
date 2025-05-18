using System.Diagnostics;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RIAYA.Models;

namespace RIAYA.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly MyDbContext _context;

        public HomeController(MyDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("IsLoggedIn")) && Request.Cookies.ContainsKey("IsLoggedIn"))
            {
                var userType = Request.Cookies["UserType"];

                if (!string.IsNullOrEmpty(userType))
                {
                    switch (userType.ToLower())
                    {
                        case "admin":
                            return RedirectToAction("AdminDashboard", "Admin");
                        case "provider":
                            return RedirectToAction("ProviderDashboard", "Provider");
                        //case "user":
                        //    return RedirectToAction("Index", "Home");
                        default:
                            break;
                    }
                }
            }

            // Get the latest 8 blogs
            var latestBlogs = _context.HealthBlogs
                .OrderByDescending(b => b.PublishDate)
                .Take(8)
                .ToList();

            ViewBag.LatestBlogs = latestBlogs;

            int totalUsers = await _context.Users.CountAsync(u => u.UserType == "user");

            // Get total providers
            int totalProviders = await _context.Providers.CountAsync();

            // Get total appointments (all types)
            int totalElectronicConsultations = await _context.ElectronicConsultations.CountAsync();
            int totalInstantHomeCareAppointments = await _context.InstantHomeCareAppointments.CountAsync();
            int totalHomeCareAppointments = await _context.HomeCareAppointments.CountAsync();
            int totalAppointments = totalElectronicConsultations + totalInstantHomeCareAppointments + totalHomeCareAppointments;

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalProviders = totalProviders;
            ViewBag.TotalAppointments = totalAppointments;
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Contact(Contact contact)
        {
            Console.WriteLine($"FullName: {contact.FullName}");
            Console.WriteLine($"Email: {contact.Email}");
            Console.WriteLine($"Subject: {contact.Subject}");
            Console.WriteLine($"Message: {contact.Message}");
            if (!ModelState.IsValid)
                return View(contact);

            contact.CreatedAt = DateTime.Now;
            _context.Contacts.Add(contact);
            await _context.SaveChangesAsync();

            var mail = new MailMessage();
            mail.From = new MailAddress("nadaqdesat@gmail.com");
            mail.To.Add("nadaqdesat@gmail.com");
            mail.Subject = $"New Contact Message from {contact.FullName}";
            mail.Body = $"Email: {contact.Email}\nSubject: {contact.Subject}\nMessage:\n{contact.Message}";

            using (var smtp = new SmtpClient("smtp.gmail.com", 587))
            {
                smtp.Credentials = new NetworkCredential("nadaqdesat@gmail.com", "aibl avfx vyag dxgn");
                smtp.EnableSsl = true;
                await smtp.SendMailAsync(mail);
            }
            ViewBag.Success = "Message sent successfully!";
            ModelState.Clear();
            return View("Contact");
        }

        public IActionResult JoinUs()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult KnowledgeCapsule()
        {
            var blogs = _context.HealthBlogs.OrderByDescending(b => b.PublishDate).ToList();
            return View(blogs);
        }

        public async Task<IActionResult> BlogDetails(int id)
        {
            var blog = await _context.HealthBlogs.FindAsync(id);

            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }
    }
}
