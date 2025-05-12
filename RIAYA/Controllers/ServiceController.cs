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

        public IActionResult Services(int? categoryId)
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
            appointment.ServiceId = selectedServiceId;
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

        [HttpGet]
        public JsonResult GetProvidersByCategory(int categoryId)
        {
            var providers = _context.Providers
                .Where(p => p.CategoryId == categoryId && p.User != null)
                .Select(p => new
                {
                    p.Id,
                    FullName = p.User.FullName,
                    p.Bio,
                    p.YearsOfExperience,
                    p.Specialization,
                    p.Location,
                    p.LicenseUrl
                })
                .ToList();

            return Json(providers);
        }
        public JsonResult GetAvailableDatesAndTimes(int providerId)
        {
            var availability = _context.ProviderAvailabilities
                .Where(a => a.ProviderId == providerId)
                .ToList();

            var today = DateTime.Today;
            var availableDates = new List<object>();

            var bookedAppointments = _context.HomeCareAppointments
                .Where(a => a.ProviderId == providerId)
                .Select(a => new { Date = a.AppointmentDate, Time = a.AppointmentTime })
                .ToList();

            for (int i = 0; i < 10; i++)
            {
                var date = today.AddDays(i);
                var dayOfWeek = (int)date.DayOfWeek;

                var dayAvailability = availability.FirstOrDefault(a => a.DayOfWeek == dayOfWeek);
                if (dayAvailability != null)
                {
                    var times = new List<string>();
                    var start = dayAvailability.StartTime;
                    var end = dayAvailability.EndTime;

                    for (var time = start; time < end; time = time.AddHours(1))
                    {
                        var dateTime = date + time.ToTimeSpan();
                        if (dateTime < DateTime.Now.AddMinutes(30))
                            continue;

                        bool isBooked = bookedAppointments.Any(b =>
                            b.Date == DateOnly.FromDateTime(date) &&
                            Math.Abs((b.Time - time).TotalMinutes) < 1);


                        if (!isBooked)
                        {
                            times.Add(time.ToString(@"hh\:mm"));
                        }
                    }

                    if (times.Any())
                    {
                        availableDates.Add(new
                        {
                            Date = date.ToString("yyyy-MM-dd"),
                            Times = times
                        });
                    }
                }
            }

            return Json(availableDates);
        }





        [HttpPost]
        public async Task<IActionResult> SubmitHomeCare(HomeCareAppointment appointment)
        {
            int patientId = 0;
            string userId = HttpContext.Session.GetString("UserId") ?? Request.Cookies["UserId"];
            if (!int.TryParse(userId, out patientId))
            {
                return Json(new { success = false, message = "User ID is missing or invalid." });
            }

            if (!int.TryParse(Request.Form["CategorySelect"], out int selectedCategoryId) ||
                !int.TryParse(Request.Form["ServiceSelect"], out int selectedServiceId) ||
                !int.TryParse(Request.Form["ProviderSelect"], out int selectedProviderId))
            {
                return Json(new { success = false, message = "Invalid selection in form data." });
            }

            var selectedCategory = _context.ServiceCategories.FirstOrDefault(c => c.Id == selectedCategoryId);
            var selectedService = _context.Services.FirstOrDefault(s => s.Id == selectedServiceId);

            if (selectedCategory == null || selectedService == null)
            {
                return Json(new { success = false, message = "Invalid category or service selected." });
            }

            appointment.PatientId = patientId;
            appointment.CategoryName = selectedCategory.CategoryName;
            appointment.ServiceName = selectedService.ServiceDescription;
            appointment.ServiceId = selectedServiceId;
            appointment.ProviderId = selectedProviderId;
            appointment.IsConfirmed = false;
            appointment.CreatedAt = DateTime.UtcNow;

            _context.HomeCareAppointments.Add(appointment);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Your appointment has been successfully created!" });
        }

        [HttpPost]
        public async Task<IActionResult> SubmitConsultation(ElectronicConsultation appointment)
        {
            // الحصول على الـ patientId من الجلسة أو الكوكيز
            int patientId = 0;
            string userId = HttpContext.Session.GetString("UserId") ?? Request.Cookies["UserId"];

            // التحقق من أن الـ userId موجود وصحيح
            if (!int.TryParse(userId, out patientId))
            {
                return Json(new { success = false, message = "User ID is missing or invalid." });
            }

            // التحقق من أن الـ CategorySelect و ServiceSelect مختارين بشكل صحيح
            if (!int.TryParse(Request.Form["consultationCategorySelect"], out int selectedCategoryId) ||
                !int.TryParse(Request.Form["consultationServiceSelect"], out int selectedServiceId))
            {
                return Json(new { success = false, message = "Invalid category or service selected." });
            }

            // الحصول على الـ category و الـ service من قاعدة البيانات
            var selectedCategory = await _context.ServiceCategories.FirstOrDefaultAsync(c => c.Id == selectedCategoryId);
            var selectedService = await _context.Services.FirstOrDefaultAsync(s => s.Id == selectedServiceId);

            // التحقق من وجود الـ category والـ service
            if (selectedCategory == null || selectedService == null)
            {
                return Json(new { success = false, message = "Invalid category or service selection." });
            }

            // تعبئة تفاصيل الاستشارة
            appointment.PatientId = patientId;
            appointment.CategoryName = selectedCategory.CategoryName;
            appointment.ServiceName = selectedService.ServiceDescription;
            appointment.ServiceId = selectedServiceId;
            appointment.IsConfirmed = false;
            appointment.ConsultationLink = "";
            appointment.CreatedAt = DateTime.UtcNow;

            _context.ElectronicConsultations.Add(appointment);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Your consultation appointment has been successfully created!" });
        }

        public IActionResult HealthcareTeam(string? specialty, string? gender, int? minExperience, int? maxExperience, string? sortBy, string? searchQuery)
        {
            // جلب جميع التخصصات (بدون فلترة)
            var allCategories = _context.ServiceCategories
                .Select(c => c.CategoryName)
                .Distinct()
                .ToList();

            // جلب جميع مقدمي الرعاية مع العلاقات المرتبطة
            var query = _context.Providers
                .Include(p => p.User)
                .Include(p => p.Category)
                .AsQueryable();

            // تطبيق الفلاتر إذا تم اختيارها
            if (!string.IsNullOrEmpty(specialty) && specialty != "All Specialty")
            {
                query = query.Where(p => p.Category.CategoryName == specialty);
            }

            if (!string.IsNullOrEmpty(gender) && gender != "No Preference")
            {
                query = query.Where(p => p.User.Gender == gender);
            }

            if (minExperience.HasValue)
            {
                query = query.Where(p => p.YearsOfExperience >= minExperience.Value);
            }

            if (maxExperience.HasValue)
            {
                query = query.Where(p => p.YearsOfExperience <= maxExperience.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                query = query.Where(p => p.User.FullName.Contains(searchQuery));
            }

            // تطبيق الترتيب حسب الخبرة أو التقييم إذا تم تحديده
            if (sortBy == "experience")
            {
                query = query.OrderByDescending(p => p.YearsOfExperience); // ترتيب تنازلي حسب الخبرة
            }
            //else if (sortBy == "rating")
            //{
            //    query = query.OrderByDescending(p => p.User.Rating); // ترتيب تنازلي حسب التقييم
            //}

            var providersWithCategories = query
                .Select(p => new HealthcareProviderViewModel
                {
                    FullName = p.User.FullName,
                    ProfileImage = p.ProfileImage,
                    Specialization = p.Specialization,
                    Bio = p.Bio,
                    YearsOfExperience = p.YearsOfExperience,
                    Location = p.Location,
                    LicenseUrl = p.LicenseUrl,
                    CategoryName = p.Category.CategoryName,
                    Availability = _context.ProviderAvailabilities
                        .Where(pa => pa.ProviderId == p.Id)
                        .ToList()
                })
                .ToList();

            // تخزين القيم في ViewData لإعادة استخدامها في الـ View
            ViewData["SelectedMinExperience"] = minExperience;
            ViewData["SelectedMaxExperience"] = maxExperience;
            ViewData["SelectedGender"] = gender;
            ViewData["AllCategories"] = allCategories;
            ViewData["SelectedCategoryName"] = specialty;
            ViewData["SelectedSortBy"] = sortBy; // حفظ القيمة المحددة للترتيب
            ViewData["SearchQuery"] = searchQuery;

            return View(providersWithCategories);
        }



    }
}
