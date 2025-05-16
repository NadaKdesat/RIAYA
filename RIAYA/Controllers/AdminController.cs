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

        [HttpGet]
        public async Task<IActionResult> GetCategory(int id)
        {
            try
            {
                var category = await _context.ServiceCategories.FindAsync(id);
                if (category == null)
                {
                    return Json(new { success = false, message = "Category not found" });
                }

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        id = category.Id,
                        categoryName = category.CategoryName,
                        categoryDescription = category.CategoryDescription
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCategory(int id, string categoryName, string categoryDescription)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(categoryName))
                {
                    return Json(new { success = false, message = "Category name is required." });
                }

                var category = await _context.ServiceCategories.FindAsync(id);
                if (category == null)
                {
                    return Json(new { success = false, message = "Category not found" });
                }

                category.CategoryName = categoryName;
                category.CategoryDescription = categoryDescription;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Category updated successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error updating category: " + ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var category = await _context.ServiceCategories
                    .Include(c => c.Services)
                    .Include(c => c.Providers)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                {
                    return Json(new { success = false, message = "Category not found" });
                }

                // Soft delete the category
                category.IsDeleted = true;

                // Soft delete all related services
                foreach (var service in category.Services)
                {
                    service.IsDeleted = true;
                }

                // Deactivate all related providers
                foreach (var provider in category.Providers)
                {
                    provider.IsActive = false;
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Category and related items have been hidden successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RestoreCategory(int id)
        {
            try
            {
                var category = await _context.ServiceCategories
                    .Include(c => c.Services)
                    .Include(c => c.Providers)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                {
                    return Json(new { success = false, message = "Category not found" });
                }

                // Restore the category
                category.IsDeleted = false;

                // Restore all related services
                foreach (var service in category.Services)
                {
                    service.IsDeleted = false;
                }

                // Reactivate all related providers
                foreach (var provider in category.Providers)
                {
                    provider.IsActive = true;
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Category and related items have been restored successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> DeletedCategories()
        {
            var categories = await _context.ServiceCategories
                .Where(c => c.IsDeleted)
                .Include(c => c.Services.Where(s => s.IsDeleted))
                .Include(c => c.Providers.Where(p => !p.IsActive))
                .ToListAsync();

            return View(categories);
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoryProviders(int id)
        {
            try
            {
                var providers = await _context.Providers
                    .Where(p => p.CategoryId == id)
                    .Include(p => p.User)
                    .Select(p => new
                    {
                        fullName = p.User.FullName,
                        email = p.User.Email,
                        phone = p.User.Phone,
                        specialization = p.Specialization,
                        yearsOfExperience = p.YearsOfExperience,
                        city = p.User.City,
                        isActive = p.IsActive
                    })
                    .ToListAsync();

                return Json(new { success = true, data = providers });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoryServices(int id)
        {
            try
            {
                var services = await _context.Services
                    .Where(s => s.CategoryId == id && !s.IsDeleted)
                    .Select(s => new
                    {
                        id = s.Id,
                        serviceType = s.ServiceType,
                        serviceDescription = s.ServiceDescription,
                        price = s.Price
                    })
                    .ToListAsync();

                return Json(new { success = true, data = services });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetService(int id)
        {
            try
            {
                var service = await _context.Services.FindAsync(id);
                if (service == null)
                {
                    return Json(new { success = false, message = "Service not found" });
                }

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        id = service.Id,
                        serviceType = service.ServiceType,
                        serviceDescription = service.ServiceDescription,
                        price = service.Price
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddService(string serviceType, string serviceDescription, decimal price, int categoryId)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(serviceType))
                {
                    return Json(new { success = false, message = "Service type is required" });
                }

                if (price <= 0)
                {
                    return Json(new { success = false, message = "Price must be greater than 0" });
                }

                // Check if category exists
                var category = await _context.ServiceCategories.FindAsync(categoryId);
                if (category == null)
                {
                    return Json(new { success = false, message = "Category not found" });
                }

                // Create new service
                var service = new Service
                {
                    ServiceType = serviceType,
                    ServiceDescription = serviceDescription,
                    Price = price,
                    CategoryId = categoryId,
                    CreatedAt = DateTime.Now,
                    IsDeleted = false
                };

                // Save to database
                await _context.Services.AddAsync(service);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Service added successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateService(int id, string serviceType, string serviceDescription, decimal price)
        {
            try
            {
                // Find service
                var service = await _context.Services.FindAsync(id);
                if (service == null)
                {
                    return Json(new { success = false, message = "Service not found" });
                }

                // Validate inputs
                if (string.IsNullOrWhiteSpace(serviceType))
                {
                    return Json(new { success = false, message = "Service type is required" });
                }

                if (price <= 0)
                {
                    return Json(new { success = false, message = "Price must be greater than 0" });
                }

                // Update service
                service.ServiceType = serviceType;
                service.ServiceDescription = serviceDescription;
                service.Price = price;

                // Save changes
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Service updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteService(int id)
        {
            try
            {
                Console.WriteLine($"Deleting service with ID: {id}");

                // First check if service exists
                var service = await _context.Services.FindAsync(id);
                if (service == null)
                {
                    Console.WriteLine($"Service not found with ID: {id}");
                    return Json(new { success = false, message = "Service not found" });
                }

                Console.WriteLine($"Found service: {service.ServiceType}, Current IsDeleted: {service.IsDeleted}");

                // Use raw SQL to update
                var sql = $"UPDATE Services SET IsDeleted = 1 WHERE Id = {id}";
                var result = await _context.Database.ExecuteSqlRawAsync(sql);
                Console.WriteLine($"SQL Update result: {result} rows affected");

                // Verify the change
                var updatedService = await _context.Services.FindAsync(id);
                Console.WriteLine($"Updated service IsDeleted status: {updatedService.IsDeleted}");

                return Json(new { success = true, message = "Service deleted successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteService: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetHiddenServices(int categoryId)
        {
            try
            {
                Console.WriteLine($"Fetching hidden services for category {categoryId}");

                // First, let's check all services in this category
                var allServices = await _context.Services
                    .Where(s => s.CategoryId == categoryId)
                    .ToListAsync();

                Console.WriteLine($"Total services in category {categoryId}: {allServices.Count}");
                foreach (var service in allServices)
                {
                    Console.WriteLine($"Service ID: {service.Id}, Type: {service.ServiceType}, IsDeleted: {service.IsDeleted}");
                }

                // Now get only the deleted services using a simpler query
                var services = await _context.Services
                    .Where(s => s.CategoryId == categoryId && s.IsDeleted == true)
                    .Select(s => new
                    {
                        id = s.Id,
                        serviceType = s.ServiceType,
                        serviceDescription = s.ServiceDescription,
                        price = s.Price,
                        isDeleted = s.IsDeleted
                    })
                    .ToListAsync();

                Console.WriteLine($"Found {services.Count} hidden services");
                foreach (var service in services)
                {
                    Console.WriteLine($"Hidden Service: {service.serviceType}, ID: {service.id}, IsDeleted: {service.isDeleted}");
                }
                return Json(new { success = true, count = services.Count, data = services });

                //return Json(new { success = true, data = services });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetHiddenServices: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RestoreService(int id)
        {
            try
            {
                var service = await _context.Services.FindAsync(id);
                if (service == null)
                {
                    return Json(new { success = false, message = "Service not found" });
                }

                service.IsDeleted = false;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Service restored successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
