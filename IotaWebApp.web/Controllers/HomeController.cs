using Microsoft.AspNetCore.Mvc;

namespace IotaWebApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}