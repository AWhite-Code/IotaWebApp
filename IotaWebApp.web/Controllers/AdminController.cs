using Microsoft.AspNetCore.Mvc;

namespace IotaWebApp.Web.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        // Add CRUD actions here - Create, Edit, Delete, etc.
    }
}
