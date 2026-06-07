using Microsoft.AspNetCore.Mvc;

namespace BagoScout.Controllers
{
    public class ApplicationsController : Controller
    {
        public IActionResult Index()
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Redirect("/?login=true");
            }

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View();
        }
    }
}
