using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RIAYA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;

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

        public async Task<IActionResult> Categories()
        {
            var categories = await _context.ServiceCategories
                .Where(c => !c.IsDeleted)
                .Include(c => c.Services.Where(s => !s.IsDeleted))
                .Include(c => c.Providers.Where(p => p.IsActive))
                .ToListAsync();

            return View(categories);
        }

        [HttpPost]
        public async Task<IActionResult> addCategory(string categoryName, string categoryDescription)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(categoryName))
                {
                    TempData["ErrorMessage"] = "Category name is required.";
                    return RedirectToAction("Categories");
                }

                var newCategory = new ServiceCategory
                {
                    CategoryName = categoryName,
                    CategoryDescription = categoryDescription,
                    CreatedAt = DateTime.Now,
                    IsDeleted = false
                };

                await _context.ServiceCategories.AddAsync(newCategory);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Category added successfully!";
                return RedirectToAction("Categories");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error adding category: {ex.Message}";
                return RedirectToAction("Categories");
            }
        }

        public async Task<IActionResult> DownloadCategoriesPDF()
        {
            var categories = await _context.ServiceCategories
                .Where(c => !c.IsDeleted)
                .Include(c => c.Services.Where(s => !s.IsDeleted))
                .Include(c => c.Providers.Where(p => p.IsActive))
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            using (MemoryStream stream = new MemoryStream())
            {
                Document document = new Document(PageSize.A4);
                PdfWriter.GetInstance(document, stream);
                document.Open();

                // Main title
                iTextSharp.text.Font titleFont = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA_BOLD, 20, BaseColor.DARK_GRAY);
                Paragraph title = new Paragraph("Service Categories Report", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20f
                };
                document.Add(title);

                // Date and time
                iTextSharp.text.Font dateFont = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA, 10, BaseColor.GRAY);
                Paragraph date = new Paragraph($"Generated on: {DateTime.Now.ToString("dd/MM/yyyy HH:mm")}", dateFont)
                {
                    Alignment = Element.ALIGN_RIGHT,
                    SpacingAfter = 20f
                };
                document.Add(date);

                // Categories table
                PdfPTable table = new PdfPTable(4)
                {
                    WidthPercentage = 100
                };

                // Set column widths
                float[] columnWidths = new float[] { 30f, 40f, 15f, 15f };
                table.SetWidths(columnWidths);

                // Table headers
                iTextSharp.text.Font headerFont = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA_BOLD, 12, BaseColor.WHITE);
                string[] headers = { "Category Name", "Description", "Services", "Providers" };

                foreach (var header in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(header, headerFont))
                    {
                        BackgroundColor = new BaseColor(5, 61, 118), // Primary color from your CSS
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 8,
                        BorderColor = BaseColor.WHITE
                    };
                    table.AddCell(cell);
                }

                // Add data rows
                iTextSharp.text.Font dataFont = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA, 10);
                iTextSharp.text.Font countFont = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA_BOLD, 10, BaseColor.DARK_GRAY);

                foreach (var category in categories)
                {
                    // Category Name
                    PdfPCell nameCell = new PdfPCell(new Phrase(category.CategoryName, dataFont))
                    {
                        HorizontalAlignment = Element.ALIGN_LEFT,
                        Padding = 8,
                        BorderColor = BaseColor.LIGHT_GRAY
                    };
                    table.AddCell(nameCell);

                    // Description
                    PdfPCell descCell = new PdfPCell(new Phrase(category.CategoryDescription ?? "No description", dataFont))
                    {
                        HorizontalAlignment = Element.ALIGN_LEFT,
                        Padding = 8,
                        BorderColor = BaseColor.LIGHT_GRAY
                    };
                    table.AddCell(descCell);

                    // Services Count
                    PdfPCell servicesCell = new PdfPCell(new Phrase(category.Services.Count.ToString(), countFont))
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 8,
                        BorderColor = BaseColor.LIGHT_GRAY
                    };
                    table.AddCell(servicesCell);

                    // Providers Count
                    PdfPCell providersCell = new PdfPCell(new Phrase(category.Providers.Count.ToString(), countFont))
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 8,
                        BorderColor = BaseColor.LIGHT_GRAY
                    };
                    table.AddCell(providersCell);
                }

                document.Add(table);

                // Add summary
                Paragraph summary = new Paragraph($"Total Categories: {categories.Count}", iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA_BOLD, 12))
                {
                    Alignment = Element.ALIGN_RIGHT,
                    SpacingBefore = 20f
                };
                document.Add(summary);

                document.Close();

                return File(stream.ToArray(), "application/pdf", "ServiceCategories.pdf");
            }
        }
    }
}
