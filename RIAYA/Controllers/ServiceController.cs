using Microsoft.AspNetCore.Mvc;

namespace RIAYA.Controllers
{
    public class ServiceController : Controller
    {
        public IActionResult Services()
        {
            return View();
        }
    }
}
