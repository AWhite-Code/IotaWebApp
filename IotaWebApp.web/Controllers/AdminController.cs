using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IotaWebApp.Data;
using IotaWebApp.Models;
using System.IO;  // For file handling
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;  // For IFormFile

namespace IotaWebApp.Controllers
{
    [Route("Admin")]  // Route prefix for all actions
    public class AdminController : Controller
    {
        private readonly WebsiteCMSDbContext _context;
        private readonly string _uploadsFolder = "uploads";

        public AdminController(WebsiteCMSDbContext context)
        {
            _context = context;
        }

        // Read: List all contents
        [HttpGet]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            try
            {
                Console.WriteLine("AdminController Index action called.");
                var contents = await _context.WebsiteContents.ToListAsync();
                return View(contents);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading content: {ex.Message}");
                return View("Error", new ErrorViewModel { ErrorMessage = "An error occurred while loading content." });
            }
        }

        // Create: Display the form to create new content
        [HttpGet]
        [Route("Create")]
        public IActionResult Create()
        {
            return View();
        }

        // Create: Handle the submission of new content, including file upload
        [HttpPost]
        [Route("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WebsiteContent content, IFormFile file)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (file != null && file.Length > 0)
                    {
                        // Save the file to the uploads folder and store its relative path in ContentValue
                        content.ContentValue = await SaveFile(file);
                    }

                    _context.WebsiteContents.Add(content);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating content: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while creating the content.");
                }
            }
            return View(content);
        }

        // Edit: Display the form to edit existing content
        [HttpGet]
        [Route("Edit/{id?}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                Console.WriteLine("Edit action: No ID provided.");
                return NotFound();
            }

            var content = await _context.WebsiteContents.FindAsync(id);
            if (content == null)
            {
                Console.WriteLine($"Edit action: Content with ID {id} not found.");
                return NotFound();
            }
            return View(content);
        }

        // Edit: Handle the submission of updated content, including file upload
        [HttpPost]
        [Route("Edit/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, WebsiteContent content, IFormFile file)
        {
            if (id != content.Id)
            {
                Console.WriteLine($"Edit action: Mismatched ID {id} for content.");
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (file != null && file.Length > 0)
                    {
                        // Save the new file and update ContentValue
                        content.ContentValue = await SaveFile(file);
                    }

                    _context.Update(content);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    Console.WriteLine($"Concurrency error while editing content: {ex.Message}");
                    if (!WebsiteContentExists(content.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error editing content: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while editing the content.");
                }
            }
            return View(content);
        }

        // Delete: Handle the deletion of content with dependency check
        [HttpPost, ActionName("Delete")]
        [Route("Delete/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var content = await _context.WebsiteContents.FindAsync(id);
            if (content == null)
            {
                Console.WriteLine($"DeleteConfirmed action: Content with ID {id} not found.");
                return NotFound();
            }

            try
            {
                // Check if content is being used (e.g., as part of the carousel)
                if (IsContentUsedInCarousel(content))
                {
                    // Replace the content in the carousel with a placeholder image path
                    ReplaceWithPlaceholder(content);
                }

                _context.WebsiteContents.Remove(content);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting content: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while deleting the content.");
                return View(content);
            }
        }

        // Method to replace content in the carousel with a placeholder
        private void ReplaceWithPlaceholder(WebsiteContent content)
        {
            // Define a default placeholder path
            string placeholderPath = "/assets/placeholder.png";  // Adjust path as needed

            // Update dependent items in the database
            var dependentItems = _context.WebsiteContents
                .Where(c => c.ContentKey.StartsWith("Carousel") && c.ContentValue == content.ContentValue)
                .ToList();

            foreach (var item in dependentItems)
            {
                item.ContentValue = placeholderPath;
            }

            _context.SaveChanges();  // Save changes to update dependencies
        }

        // Method to check if content is used in the carousel
        private bool IsContentUsedInCarousel(WebsiteContent content)
        {
            // Check if the content key matches keys used in the carousel
            return content.ContentKey == "HeroImage" || content.ContentKey.StartsWith("Carousel");
        }

        // Utility method to save uploaded files
        private async Task<string> SaveFile(IFormFile file)
        {
            try
            {
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", _uploadsFolder);
                Directory.CreateDirectory(uploadPath);  // Ensure the uploads folder exists

                var fileName = Path.GetFileName(file.FileName);
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return $"/{_uploadsFolder}/{fileName}";  // Return the relative path for storage in the database
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving file: {ex.Message}");
                return null;  // Return null if there's an error
            }
        }

        private bool WebsiteContentExists(int id)
        {
            return _context.WebsiteContents.Any(e => e.Id == id);
        }
    }
}
