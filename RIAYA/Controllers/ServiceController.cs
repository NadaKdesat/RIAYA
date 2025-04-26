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

        public IActionResult HomeVisitOptions()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitInstantCare(InstantHomeCareAppointment appointment)
        {
            appointment.Status = "Pending";
            appointment.IsConfirmed = true;
            appointment.CreatedAt = DateTime.Now;

            _context.InstantHomeCareAppointments.Add(appointment);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Appointment Confirmed" });
        }


    }
}
