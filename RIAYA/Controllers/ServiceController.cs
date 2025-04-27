using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RIAYA.Models;

namespace RIAYA.Controllers
{
    public class ServiceController : Controller
    {
        private readonly MyDbContext _context;

        public ServiceController(MyDbContext context)
        {
            _context = context;
        }

        public IActionResult Services()
        {
            return View();
        }

        public IActionResult HomeVisitOptions(int? categoryId)
        {
            var categoriesWithServices = _context.ServiceCategories
                .Include(c => c.Services)
                .ToList();

            if (categoryId.HasValue)
            {
                var selectedCategory = categoriesWithServices.FirstOrDefault(c => c.Id == categoryId);
                if (selectedCategory != null)
                {
                    ViewData["SelectedCategory"] = selectedCategory;
                }
            }

            return View(categoriesWithServices);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitInstantCare(InstantHomeCareAppointment appointment)
        {
            // الحصول على PatientId
            int patientId = 0;
            string userIdFromSession = HttpContext.Session.GetString("UserId");
            string userIdFromCookie = Request.Cookies["UserId"];

            if (!string.IsNullOrEmpty(userIdFromSession) && int.TryParse(userIdFromSession, out patientId))
            {
                ViewData["PatientId"] = patientId;
            }
            else if (!string.IsNullOrEmpty(userIdFromCookie) && int.TryParse(userIdFromCookie, out patientId))
            {
                ViewData["PatientId"] = patientId;
            }
            else
            {
                ViewBag.ErrorMessage = "User ID is missing or invalid.";
                return View(appointment);
            }

            // جلب الـ Category و الـ Service من القيم المختارة في الـ HTML
            var selectedCategoryId = int.Parse(Request.Form["CategorySelect"]);
            var selectedServiceId = int.Parse(Request.Form["ServiceSelect"]);

            // استرجاع Category و Service من قاعدة البيانات بناءً على الـ Id
            var selectedCategory = _context.ServiceCategories.FirstOrDefault(c => c.Id == selectedCategoryId);
            var selectedService = _context.Services.FirstOrDefault(s => s.Id == selectedServiceId);

            if (selectedCategory != null && selectedService != null)
            {
                appointment.CategoryName = selectedCategory.CategoryName; // تخزين اسم الفئة
                appointment.ServiceName = selectedService.ServiceDescription; // تخزين اسم الخدمة
            }
            else
            {
                ViewBag.ErrorMessage = "Invalid category or service selected.";
                return View(appointment);
            }

            // تعيين القيم الأخرى
            appointment.PatientId = patientId;
            appointment.Status = "Pending";
            appointment.IsConfirmed = false;
            appointment.CreatedAt = DateTime.UtcNow;

            // إضافة الحجز إلى قاعدة البيانات
            _context.InstantHomeCareAppointments.Add(appointment);
            await _context.SaveChangesAsync();

            // إرسال الاستجابة بشكل JSON بدلاً من إستخدام ViewBag
            return Json(new { success = true, message = "Your appointment has been successfully created!" });
        }

        [HttpGet]
        public JsonResult GetServicesByCategory(int categoryId)
        {
            var services = _context.Services
                .Where(s => s.CategoryId == categoryId)
                .Select(s => new { s.Id, s.ServiceDescription, s.Price })
                .ToList();

            return Json(services);
        }

    }
}
