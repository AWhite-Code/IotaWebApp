using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IotaWebApp.Data;
using IotaWebApp.Models;

namespace IotaWebApp.Controllers
{
    [Route("Admin")]
    public class AdminController : Controller
    {
        private readonly WebsiteCMSDbContext _context;
        private readonly ILogger<AdminController> _logger; 
        private readonly string _uploadsFolder = "uploads";

        public AdminController(WebsiteCMSDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Read: List all contents
        [HttpGet]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("AdminController Index action called.");
                var contents = await _context.WebsiteContents.ToListAsync();
                return View(contents);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading content: {ex.Message}");
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
                        content.ContentValue = await SaveFile(file);
                    }

                    _context.WebsiteContents.Add(content);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Content created successfully.");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error creating content: {ex.Message}");
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
                _logger.LogWarning("Edit action: No ID provided.");
                return NotFound();
            }

            var content = await _context.WebsiteContents.FindAsync(id);
            if (content == null)
            {
                _logger.LogWarning($"Edit action: Content with ID {id} not found.");
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
                _logger.LogWarning($"Edit action: Mismatched ID {id} for content.");
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
                    _logger.LogInformation("Content edited successfully.");
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError($"Concurrency error while editing content: {ex.Message}");
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
                    _logger.LogError($"Error editing content: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while editing the content.");
                }
            }
            return View(content);
        }

        // Delete: Handle the deletion of content with dependency check
        [HttpPost]
        [Route("Delete/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var content = await _context.WebsiteContents.FindAsync(id);
                if (content == null)
                {
                    _logger.LogWarning($"DeleteConfirmed action: Content with ID {id} not found.");
                    return NotFound();  // Return a NotFound view if content does not exist
                }

                _logger.LogInformation($"Attempting to delete content with ID {id} and ContentKey {content.ContentKey}.");

                // Check if content is being used (e.g., as part of the carousel)
                if (IsContentUsedInCarousel(content))
                {
                    _logger.LogInformation("Content is used in carousel; replacing with placeholder.");
                    ReplaceWithPlaceholder(content);
                }

                _context.WebsiteContents.Remove(content);
                await _context.SaveChangesAsync();  // Ensure changes are saved to the database

                _logger.LogInformation("Content successfully deleted and changes saved.");

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting content: {ex.Message} - StackTrace: {ex.StackTrace}");
                ModelState.AddModelError("", "An error occurred while deleting the content.");

                return View("Delete", new WebsiteContent { Id = id });
            }
        }

        // Method to replace content in the carousel with a placeholder
        private void ReplaceWithPlaceholder(WebsiteContent content)
        {
            try
            {
                string placeholderPath = "/assets/placeholder.png";

                _logger.LogInformation($"Replacing content '{content.ContentValue}' with placeholder '{placeholderPath}'.");

                var dependentItems = _context.WebsiteContents
                    .Where(c => c.ContentKey.StartsWith("Carousel") && c.ContentValue == content.ContentValue)
                    .ToList();

                foreach (var item in dependentItems)
                {
                    _logger.LogInformation($"Replacing dependent item with ID {item.Id}.");
                    item.ContentValue = placeholderPath;
                }

                _context.SaveChanges(); 
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error replacing content with placeholder: {ex.Message}");
            }
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
                _logger.LogError($"Error saving file: {ex.Message}");
                return null;
            }
        }

        private bool WebsiteContentExists(int id)
        {
            return _context.WebsiteContents.Any(e => e.Id == id);
        }
    }
}
