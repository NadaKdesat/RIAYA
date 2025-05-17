using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RIAYA.Models;
using System.Security.Cryptography;
using System.Text;

namespace RIAYA.Controllers
{
    public class ProviderController : Controller
    {
        private readonly MyDbContext _context;

        public ProviderController(MyDbContext context)
        {
            _context = context;
        }
        //ProviderDashboard
        public async Task<IActionResult> ProviderDashboard()
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId") ?? Request.Cookies["UserId"];
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "User");
                }

                var provider = await _context.Providers
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.UserId == int.Parse(userId));

                if (provider == null)
                {
                    return NotFound();
                }

                var currentDate = DateTime.Now;

                // Get upcoming appointments count
                var upcomingAppointments = await _context.InstantHomeCareAppointments
                    .Where(a => a.ProviderId == provider.Id && a.CreatedAt >= currentDate)
                    .CountAsync();

                upcomingAppointments += await _context.HomeCareAppointments
                    .Where(a => a.ProviderId == provider.Id &&
                           (a.AppointmentDate > DateOnly.FromDateTime(currentDate) ||
                            (a.AppointmentDate == DateOnly.FromDateTime(currentDate) &&
                             a.AppointmentTime > TimeOnly.FromDateTime(currentDate))))
                    .CountAsync();

                upcomingAppointments += await _context.ElectronicConsultations
                    .Where(a => a.ProviderId == provider.Id &&
                           (a.AppointmentDate > DateOnly.FromDateTime(currentDate) ||
                            (a.AppointmentDate == DateOnly.FromDateTime(currentDate) &&
                             a.AppointmentTime > TimeOnly.FromDateTime(currentDate))))
                    .CountAsync();

                // Get total appointments count
                var totalInstantAppointments = await _context.InstantHomeCareAppointments
                    .Where(a => a.ProviderId == provider.Id)
                    .CountAsync();

                var totalHomeCareAppointments = await _context.HomeCareAppointments
                    .Where(a => a.ProviderId == provider.Id)
                    .CountAsync();

                var totalElectronicConsultations = await _context.ElectronicConsultations
                    .Where(a => a.ProviderId == provider.Id)
                    .CountAsync();

                // Calculate total earnings (90% of appointment prices)
                var instantEarnings = await _context.InstantHomeCareAppointments
                    .Where(a => a.ProviderId == provider.Id)
                    .Include(a => a.Service)
                    .Select(a => a.Service != null ? a.Service.Price : 0)
                    .SumAsync() * 0.9m;

                var homeCareEarnings = await _context.HomeCareAppointments
                    .Where(a => a.ProviderId == provider.Id)
                    .Include(a => a.Service)
                    .Select(a => a.Service != null ? a.Service.Price : 0)
                    .SumAsync() * 0.9m;

                var electronicEarnings = await _context.ElectronicConsultations
                    .Where(a => a.ProviderId == provider.Id)
                    .Include(a => a.Service)
                    .Select(a => a.Service != null ? a.Service.Price : 0)
                    .SumAsync() * 0.9m;

                var totalEarnings = instantEarnings + homeCareEarnings + electronicEarnings;

                //Get provider rating
                var ratings = await _context.SessionRatings
                    .Where(r =>
                        (r.AppointmentType == "InstantHomeCareAppointments" &&
                         _context.InstantHomeCareAppointments.Any(a => a.Id == r.AppointmentId && a.ProviderId == provider.Id)) ||
                        (r.AppointmentType == "HomeCareAppointments" &&
                         _context.HomeCareAppointments.Any(a => a.Id == r.AppointmentId && a.ProviderId == provider.Id)) ||
                        (r.AppointmentType == "ElectronicConsultations" &&
                         _context.ElectronicConsultations.Any(a => a.Id == r.AppointmentId && a.ProviderId == provider.Id))
                    )
                    .ToListAsync();

                var averageRating = ratings.Any() ? ratings.Average(r => r.Rating) : 0;

                //Get available instant appointments in provider's category
                var availableInstantAppointments = await _context.InstantHomeCareAppointments
                    .Where(a => a.ProviderId == null &&
                               a.Service.CategoryId == provider.CategoryId &&
                               a.CreatedAt >= currentDate)
                    .OrderBy(a => a.CreatedAt)
                    .ToListAsync();

                // Get provider availability
                var availability = await _context.ProviderAvailabilities
                    .Where(a => a.ProviderId == provider.Id)
                    .OrderBy(a => a.DayOfWeek)
                    .ThenBy(a => a.StartTime)
                    .ToListAsync();

                ViewBag.Provider = provider;
                ViewBag.UpcomingAppointments = upcomingAppointments;
                ViewBag.TotalInstantAppointments = totalInstantAppointments;
                ViewBag.TotalHomeCareAppointments = totalHomeCareAppointments;
                ViewBag.TotalElectronicConsultations = totalElectronicConsultations;
                ViewBag.TotalEarnings = totalEarnings;
                ViewBag.AverageRating = averageRating;
                ViewBag.AvailableInstantAppointments = availableInstantAppointments;
                ViewBag.Availability = availability;

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading dashboard: {ex.Message}";
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> AcceptInstantAppointment(int appointmentId)
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId") ?? Request.Cookies["UserId"];
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User not found" });
                }

                var provider = await _context.Providers
                    .FirstOrDefaultAsync(p => p.UserId == int.Parse(userId));

                if (provider == null)
                {
                    return Json(new { success = false, message = "Provider not found" });
                }

                var appointment = await _context.InstantHomeCareAppointments
                    .FirstOrDefaultAsync(a => a.Id == appointmentId && a.ProviderId == null);

                if (appointment == null)
                {
                    return Json(new { success = false, message = "Appointment not found or already assigned" });
                }

                appointment.ProviderId = provider.Id;
                appointment.IsConfirmed = true;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Appointment accepted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAvailability([FromBody] List<ProviderAvailability> availability)
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId") ?? Request.Cookies["UserId"];
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User not found" });
                }

                var provider = await _context.Providers
                    .FirstOrDefaultAsync(p => p.UserId == int.Parse(userId));

                if (provider == null)
                {
                    return Json(new { success = false, message = "Provider not found" });
                }

                // Remove existing availability
                var existingAvailability = await _context.ProviderAvailabilities
                    .Where(a => a.ProviderId == provider.Id)
                    .ToListAsync();

                _context.ProviderAvailabilities.RemoveRange(existingAvailability);

                // Add new availability
                foreach (var slot in availability)
                {
                    slot.ProviderId = provider.Id;
                    _context.ProviderAvailabilities.Add(slot);
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Availability updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        //ProviderAppointments
        public async Task<IActionResult> ProviderAppointments()
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId") ?? Request.Cookies["UserId"];
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "User");
                }

                var providerId = await _context.Providers
                    .Where(p => p.UserId == int.Parse(userId))
                    .Select(p => p.Id)
                    .FirstOrDefaultAsync();

                if (providerId == 0)
                {
                    return NotFound();
                }

                var currentDate = DateTime.Now;

                // Fetch upcoming appointments
                var upcomingInstantAppointments = await _context.InstantHomeCareAppointments
                    .Where(a => a.ProviderId == providerId && a.CreatedAt >= currentDate)
                    .OrderBy(a => a.CreatedAt)
                    .ToListAsync();

                var upcomingHomeCareAppointments = await _context.HomeCareAppointments
                    .Where(a => a.ProviderId == providerId &&
                           (a.AppointmentDate > DateOnly.FromDateTime(currentDate) ||
                            (a.AppointmentDate == DateOnly.FromDateTime(currentDate) &&
                             a.AppointmentTime > TimeOnly.FromDateTime(currentDate))))
                    .OrderBy(a => a.AppointmentDate)
                    .ThenBy(a => a.AppointmentTime)
                    .ToListAsync();

                var upcomingElectronicConsultations = await _context.ElectronicConsultations
                    .Where(a => a.ProviderId == providerId &&
                           (a.AppointmentDate > DateOnly.FromDateTime(currentDate) ||
                            (a.AppointmentDate == DateOnly.FromDateTime(currentDate) &&
                             a.AppointmentTime > TimeOnly.FromDateTime(currentDate))))
                    .OrderBy(a => a.AppointmentDate)
                    .ThenBy(a => a.AppointmentTime)
                    .ToListAsync();

                // Fetch past appointments
                var pastInstantAppointments = await _context.InstantHomeCareAppointments
                    .Where(a => a.ProviderId == providerId && a.CreatedAt < currentDate)
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();

                var pastHomeCareAppointments = await _context.HomeCareAppointments
                    .Where(a => a.ProviderId == providerId &&
                           (a.AppointmentDate < DateOnly.FromDateTime(currentDate) ||
                            (a.AppointmentDate == DateOnly.FromDateTime(currentDate) &&
                             a.AppointmentTime < TimeOnly.FromDateTime(currentDate))))
                    .OrderByDescending(a => a.AppointmentDate)
                    .ThenByDescending(a => a.AppointmentTime)
                    .ToListAsync();

                var pastElectronicConsultations = await _context.ElectronicConsultations
                    .Where(a => a.ProviderId == providerId &&
                           (a.AppointmentDate < DateOnly.FromDateTime(currentDate) ||
                            (a.AppointmentDate == DateOnly.FromDateTime(currentDate) &&
                             a.AppointmentTime < TimeOnly.FromDateTime(currentDate))))
                    .OrderByDescending(a => a.AppointmentDate)
                    .ThenByDescending(a => a.AppointmentTime)
                    .ToListAsync();

                ViewBag.UpcomingInstantAppointments = upcomingInstantAppointments;
                ViewBag.UpcomingHomeCareAppointments = upcomingHomeCareAppointments;
                ViewBag.UpcomingElectronicConsultations = upcomingElectronicConsultations;
                ViewBag.PastInstantAppointments = pastInstantAppointments;
                ViewBag.PastHomeCareAppointments = pastHomeCareAppointments;
                ViewBag.PastElectronicConsultations = pastElectronicConsultations;

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading appointments: {ex.Message}";
                return View();
            }
        }

        public async Task<IActionResult> GetAppointmentDetails(string type, int id)
        {
            try
            {
                switch (type.ToLower())
                {
                    case "instant":
                        var instantAppointment = await _context.InstantHomeCareAppointments
                            .Include(a => a.Patient)
                            .FirstOrDefaultAsync(a => a.Id == id);
                        if (instantAppointment != null)
                        {
                            return PartialView("_InstantAppointmentDetails", instantAppointment);
                        }
                        break;

                    case "homecare":
                        var homeCareAppointment = await _context.HomeCareAppointments
                            .Include(a => a.Patient)
                            .FirstOrDefaultAsync(a => a.Id == id);
                        if (homeCareAppointment != null)
                        {
                            return PartialView("_HomeCareAppointmentDetails", homeCareAppointment);
                        }
                        break;

                    case "electronic":
                        var electronicAppointment = await _context.ElectronicConsultations
                            .Include(a => a.Patient)
                            .FirstOrDefaultAsync(a => a.Id == id);
                        if (electronicAppointment != null)
                        {
                            return PartialView("_ElectronicAppointmentDetails", electronicAppointment);
                        }
                        break;
                }

                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //ProviderProfile
        public async Task<IActionResult> ProviderProfile()
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId") ?? Request.Cookies["UserId"];
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "User");
                }

                var provider = await _context.Providers
                    .Include(p => p.User)
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.UserId == int.Parse(userId));

                if (provider == null)
                {
                    return NotFound();
                }

                return View(provider);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading profile: {ex.Message}";
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(string fullName, string email, string phone, string city,
            string specialization, int yearsOfExperience, string bio, IFormFile profileImage)
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId") ?? Request.Cookies["UserId"];
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User not found" });
                }

                var provider = await _context.Providers
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.UserId == int.Parse(userId));

                if (provider == null)
                {
                    return Json(new { success = false, message = "Provider not found" });
                }

                // Update User data
                provider.User.FullName = fullName;
                provider.User.Email = email;
                provider.User.Phone = phone;
                provider.User.City = city;

                // Update Provider data
                provider.Specialization = specialization;
                provider.YearsOfExperience = yearsOfExperience;
                provider.Bio = bio;

                // Handle profile image upload
                if (profileImage != null && profileImage.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(profileImage.FileName);
                    var imageDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    if (!Directory.Exists(imageDirectory))
                    {
                        Directory.CreateDirectory(imageDirectory);
                    }

                    var fullPath = Path.Combine(imageDirectory, fileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await profileImage.CopyToAsync(stream);
                    }

                    provider.ProfileImage = fileName;

                    // Update session and cookies
                    HttpContext.Session.SetString("ProfileImage", fileName);
                    Response.Cookies.Append("ProfileImage", fileName, new CookieOptions
                    {
                        Expires = DateTime.Now.AddDays(7),
                        HttpOnly = true,
                        Secure = true
                    });
                }

                await _context.SaveChangesAsync();

                // Update session and cookies
                HttpContext.Session.SetString("FullName", fullName);
                HttpContext.Session.SetString("Specialization", specialization);
                Response.Cookies.Append("FullName", fullName, new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(7),
                    HttpOnly = true,
                    Secure = true
                });
                Response.Cookies.Append("Specialization", specialization, new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(7),
                    HttpOnly = true,
                    Secure = true
                });

                return Json(new { success = true, message = "Profile updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            try
            {
                if (model == null || string.IsNullOrEmpty(model.CurrentPassword) || string.IsNullOrEmpty(model.NewPassword))
                {
                    return Json(new { success = false, message = "Please provide all required fields" });
                }

                var userId = HttpContext.Session.GetString("UserId") ?? Request.Cookies["UserId"];
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User not found" });
                }

                var user = await _context.Users.FindAsync(int.Parse(userId));
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                // Verify current password
                if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash))
                {
                    return Json(new { success = false, message = "Current password is incorrect" });
                }

                // Update password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
