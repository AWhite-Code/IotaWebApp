using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IotaWebApp.Data;
using IotaWebApp.Models;
using System.Threading.Tasks;
using System.Linq;

namespace IotaWebApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly WebsiteCMSDbContext _context;

        public AdminController(WebsiteCMSDbContext context)
        {
            _context = context;
        }

        // Read: List all contents
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var contents = await _context.WebsiteContents.ToListAsync();
            return View(contents);
        }

        // Create: Display the form to create new content
        [HttpGet]
        public IActionResult CreateContent()
        {
            return View();
        }

        // Create: Handle the submission of new content
        [HttpPost]
        public async Task<IActionResult> CreateContent(WebsiteContent content)
        {
            if (ModelState.IsValid)
            {
                _context.Add(content);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(content);
        }

        // Update: Display the form to edit existing content
        [HttpGet]
        public async Task<IActionResult> EditContent(int id)
        {
            var content = await _context.WebsiteContents.FindAsync(id);
            if (content == null)
            {
                return NotFound();
            }
            return View(content);
        }

        // Update: Handle the submission of the edited content
        [HttpPost]
        public async Task<IActionResult> EditContent(WebsiteContent content)
        {
            if (ModelState.IsValid)
            {
                _context.Update(content);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(content);
        }

        // Delete: Display the form to confirm content deletion
        [HttpGet]
        public async Task<IActionResult> DeleteContent(int id)
        {
            var content = await _context.WebsiteContents.FindAsync(id);
            if (content == null)
            {
                return NotFound();
            }
            return View(content);
        }

        // Delete: Handle the deletion of content
        [HttpPost, ActionName("DeleteContent")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var content = await _context.WebsiteContents.FindAsync(id);
            if (content != null)
            {
                _context.WebsiteContents.Remove(content);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
