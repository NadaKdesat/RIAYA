using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RIAYA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RIAYA.Controllers
{
    public class AdminController : Controller
    {
        private readonly MyDbContext _context;

        public AdminController(MyDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> AdminDashboard()
        {
            try
            {
                // Get total users
                int totalUsers = await _context.Users.CountAsync(u => u.UserType == "user");

                // Get total providers
                int totalProviders = await _context.Providers.CountAsync();

                // Get total appointments (all types)
                int totalElectronicConsultations = await _context.ElectronicConsultations.CountAsync();
                int totalInstantHomeCareAppointments = await _context.InstantHomeCareAppointments.CountAsync();
                int totalHomeCareAppointments = await _context.HomeCareAppointments.CountAsync();
                int totalAppointments = totalElectronicConsultations + totalInstantHomeCareAppointments + totalHomeCareAppointments;

                // Get today's appointments
                DateTime today = DateTime.Today;

                // Get today's electronic consultations
                var todayElectronicConsultationsList = await _context.ElectronicConsultations.ToListAsync();
                int todayElectronicConsultations = todayElectronicConsultationsList
                    .Count(ec => ec.CreatedAt.HasValue && ec.CreatedAt.Value.Date == today);

                // Get today's instant home care appointments
                var todayInstantHomeCareList = await _context.InstantHomeCareAppointments.ToListAsync();
                int todayInstantHomeCareAppointments = todayInstantHomeCareList
                    .Count(ihc => ihc.CreatedAt.HasValue && ihc.CreatedAt.Value.Date == today);

                // Get today's home care appointments
                var todayHomeCareList = await _context.HomeCareAppointments.ToListAsync();
                int todayHomeCareAppointments = todayHomeCareList
                    .Count(hc => hc.CreatedAt.HasValue && hc.CreatedAt.Value.Date == today);

                int todayAppointments = todayElectronicConsultations + todayInstantHomeCareAppointments + todayHomeCareAppointments;

                // Calculate total earnings (assuming 10% commission)
                var electronicConsultationsList = await _context.ElectronicConsultations
                    .Include(ec => ec.Service)
                    .ToListAsync();
                decimal electronicConsultationEarnings = electronicConsultationsList
                    .Where(ec => ec.Service != null)
                    .Sum(ec => (ec.Service.Price ?? 0) * 0.1m);

                var instantHomeCareList = await _context.InstantHomeCareAppointments
                    .Include(ihc => ihc.Service)
                    .ToListAsync();
                decimal instantHomeCareEarnings = instantHomeCareList
                    .Where(ihc => ihc.Service != null)
                    .Sum(ihc => (ihc.Service.Price ?? 0) * 0.1m);

                var homeCareList = await _context.HomeCareAppointments
                    .Include(hc => hc.Service)
                    .ToListAsync();
                decimal homeCareEarnings = homeCareList
                    .Where(hc => hc.Service != null)
                    .Sum(hc => (hc.Service.Price ?? 0) * 0.1m);

                decimal totalEarnings = electronicConsultationEarnings + instantHomeCareEarnings + homeCareEarnings;

                // Get average session rating
                var recentRatingsList = await _context.SessionRatings
                    .OrderByDescending(sr => sr.CreatedAt)
                    .Take(10)
                    .ToListAsync();
                double averageRating = recentRatingsList.Any() ? recentRatingsList.Average(sr => sr.Rating) : 0;

                // Get user growth data for the last 7 days
                var userGrowthData = new List<object>();
                var allUsers = await _context.Users.ToListAsync();

                for (int i = 6; i >= 0; i--)
                {
                    DateTime date = today.AddDays(-i);
                    int newUsers = allUsers.Count(u => u.CreatedAt.HasValue && u.CreatedAt.Value.Date == date);
                    userGrowthData.Add(new { date = date.ToString("MMM dd"), count = newUsers });
                }

                // Get appointments by type for the last 30 days
                DateTime thirtyDaysAgo = today.AddDays(-30);

                var electronicConsultationsByDay = new List<object>();
                var electronicConsultationsGrouped = todayElectronicConsultationsList
                    .Where(ec => ec.CreatedAt.HasValue && ec.CreatedAt.Value.Date >= thirtyDaysAgo)
                    .GroupBy(ec => ec.CreatedAt.Value.Date)
                    .OrderBy(g => g.Key);

                foreach (var group in electronicConsultationsGrouped)
                {
                    electronicConsultationsByDay.Add(new { date = group.Key.ToString("MMM dd"), count = group.Count() });
                }

                var instantHomeCareByDay = new List<object>();
                var instantHomeCareGrouped = todayInstantHomeCareList
                    .Where(ihc => ihc.CreatedAt.HasValue && ihc.CreatedAt.Value.Date >= thirtyDaysAgo)
                    .GroupBy(ihc => ihc.CreatedAt.Value.Date)
                    .OrderBy(g => g.Key);

                foreach (var group in instantHomeCareGrouped)
                {
                    instantHomeCareByDay.Add(new { date = group.Key.ToString("MMM dd"), count = group.Count() });
                }

                var homeCareByDay = new List<object>();
                var homeCareGrouped = todayHomeCareList
                    .Where(hc => hc.CreatedAt.HasValue && hc.CreatedAt.Value.Date >= thirtyDaysAgo)
                    .GroupBy(hc => hc.CreatedAt.Value.Date)
                    .OrderBy(g => g.Key);

                foreach (var group in homeCareGrouped)
                {
                    homeCareByDay.Add(new { date = group.Key.ToString("MMM dd"), count = group.Count() });
                }

                // Get recent session ratings
                var recentRatings = await _context.SessionRatings
                    .OrderByDescending(sr => sr.CreatedAt)
                    .Take(5)
                    .ToListAsync();

                // Prepare data for the view
                ViewBag.TotalUsers = totalUsers;
                ViewBag.TotalProviders = totalProviders;
                ViewBag.TotalAppointments = totalAppointments;
                ViewBag.TotalEarnings = totalEarnings;
                ViewBag.AverageRating = averageRating;
                ViewBag.TodayAppointments = todayAppointments;
                ViewBag.UserGrowthData = userGrowthData;
                ViewBag.ElectronicConsultationsByDay = electronicConsultationsByDay;
                ViewBag.InstantHomeCareByDay = instantHomeCareByDay;
                ViewBag.HomeCareByDay = homeCareByDay;
                ViewBag.RecentRatings = recentRatings;

                // Appointment type distribution for pie chart
                ViewBag.AppointmentTypeDistribution = new[]
                {
                    new { type = "Electronic Consultations", count = totalElectronicConsultations },
                    new { type = "Instant Home Care", count = totalInstantHomeCareAppointments },
                    new { type = "Home Care", count = totalHomeCareAppointments }
                };

                return View();
            }
            catch (Exception ex)
            {
                // Log the exception
                ViewBag.ErrorMessage = $"Error: {ex.Message}\nStackTrace: {ex.StackTrace}";
                return View();
            }
        }

        public async Task<IActionResult> Providers()
        {
            try
            {
                var providers = await _context.Providers
                    .Include(p => p.User)
                    .Include(p => p.Category)
                    .Include(p => p.Certificates)
                    .ToListAsync();

                return View(providers);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error: {ex.Message}";
                return View(new List<Provider>());
            }
        }

        public async Task<IActionResult> Users()
        {
            try
            {
                var users = await _context.Users
                    .Include(u => u.Provider)
                    .ToListAsync();

                return View(users);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error: {ex.Message}";
                return View(new List<User>());
            }
        }
        public async Task<IActionResult> Categories()
        {
            try
            {
                var categories = await _context.ServiceCategories
                    .Include(c => c.Services)
                    .Include(c => c.Providers)
                    .ToListAsync();

                return View(categories);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error: {ex.Message}";
                return View(new List<ServiceCategory>());
            }
        }
        public async Task<IActionResult> Reservations()
        {
            try
            {
                var electronicConsultations = await _context.ElectronicConsultations
                    .Include(ec => ec.Patient)
                    .Include(ec => ec.Provider)
                    .Include(ec => ec.Service)
                    .ToListAsync();

                var homeCareAppointments = await _context.HomeCareAppointments
                    .Include(hc => hc.Patient)
                    .Include(hc => hc.Provider)
                    .Include(hc => hc.Service)
                    .ToListAsync();

                var instantHomeCareAppointments = await _context.InstantHomeCareAppointments
                    .Include(ihc => ihc.Patient)
                    .Include(ihc => ihc.Provider)
                    .Include(ihc => ihc.Service)
                    .ToListAsync();

                ViewBag.ElectronicConsultations = electronicConsultations;
                ViewBag.HomeCareAppointments = homeCareAppointments;
                ViewBag.InstantHomeCareAppointments = instantHomeCareAppointments;

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error: {ex.Message}";
                return View();
            }
        }
        public async Task<IActionResult> ContactUs()
        {
            try
            {
                var contacts = await _context.Contacts
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                return View(contacts);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error: {ex.Message}";
                return View(new List<Contact>());
            }
        }
        public async Task<IActionResult> HealthBlogs()
        {
            try
            {
                var blogs = await _context.HealthBlogs
                    .OrderByDescending(b => b.PublishDate)
                    .ToListAsync();

                return View(blogs);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error: {ex.Message}";
                return View(new List<HealthBlog>());
            }
        }

        public async Task<IActionResult> Services()
        {
                return View();
        }
    }
}
