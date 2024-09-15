using Microsoft.AspNetCore.Mvc;
using IotaWebApp.Data;
using Microsoft.EntityFrameworkCore;

namespace IotaWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly WebsiteCMSDbContext _context;

        public HomeController(WebsiteCMSDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var contentList = await _context.WebsiteContents.ToListAsync();
            return View(contentList);
        }
    }
}
