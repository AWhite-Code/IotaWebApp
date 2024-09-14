using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IotaWebApp.Data;
using IotaWebApp.Models;

namespace IotaWebApp.Controllers
{
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

        // GET: Admin/Index
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

        // GET: Admin/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WebsiteContent content, IFormFile file)
        {
            // Clear pre-existing content
            ModelState.Remove("file");
            ModelState.Remove("ContentValue");

            if (content.ContentType == "Image")
            {
                if (file != null && file.Length > 0)
                {
                    content.ContentValue = await SaveFile(file);
                }
                else
                {
                    ModelState.AddModelError("file", "An image file is required for Image content type.");
                }
            }
            else if (content.ContentType == "Text")
            {
                if (string.IsNullOrWhiteSpace(content.ContentValue))
                {
                    ModelState.AddModelError("ContentValue", "Content Value is required for Text content type.");
                }
            }
            else
            {
                ModelState.AddModelError("ContentType", "Please select a valid Content Type.");
            }

            if (ModelState.IsValid)
            {
                try
                {
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
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _logger.LogWarning("ModelState is invalid: " + string.Join("; ", errors));
            }

            return View(content);
        }

        // GET: Admin/Edit/5
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

        // POST: Admin/Edit/5
        // POST: Admin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, WebsiteContent content, IFormFile file)
        {
            if (id != content.Id)
            {
                return NotFound();
            }

            // Fetch the existing content from the database to compare and update
            var existingContent = await _context.WebsiteContents.AsNoTracking().FirstOrDefaultAsync(c => c.Id == content.Id);
            if (existingContent == null)
            {
                _logger.LogWarning($"Edit action: Content with ID {id} not found.");
                return NotFound();
            }

            // Clear any existing errors for 'file' to avoid false validation failures
            ModelState.Remove("file");

            // Handle Image content type
            if (content.ContentType == "Image")
            {
                if (file != null && file.Length > 0)
                {
                    // Save the new image file and update ContentValue
                    content.ContentValue = await SaveFile(file);
                }
                else
                {
                    // If no new file is uploaded, keep the existing ContentValue
                    if (string.IsNullOrEmpty(existingContent.ContentValue))
                    {
                        // If there's no existing ContentValue, require a file
                        ModelState.AddModelError("file", "An image file is required for Image content type.");
                    }
                    else
                    {
                        // Keep the existing ContentValue
                        content.ContentValue = existingContent.ContentValue;
                    }
                }
            }
            // Handle Text content type
            else if (content.ContentType == "Text")
            {
                if (string.IsNullOrWhiteSpace(content.ContentValue))
                {
                    ModelState.AddModelError("ContentValue", "Content Value is required for Text content type.");
                }
            }
            // Handle invalid ContentType
            else
            {
                ModelState.AddModelError("ContentType", "Please select a valid Content Type.");
            }

            // Check model state validity
            if (ModelState.IsValid)
            {
                try
                {
                    // Update the content in the database
                    _context.Update(content);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Content updated successfully.");
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WebsiteContentExists(content.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            else
            {
                // Log model state errors
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _logger.LogWarning("ModelState is invalid: " + string.Join("; ", errors));
            }

            // If we reach this point, something went wrong, so return the view with the existing content
            return View(content);
        }


        private bool WebsiteContentExists(int id)
        {
            return _context.WebsiteContents.Any(e => e.Id == id);
        }

        // GET: Admin/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (id == 0)
            {
                _logger.LogWarning("Delete action: No ID provided.");
                return NotFound();
            }

            var content = await _context.WebsiteContents.FindAsync(id);
            if (content == null)
            {
                _logger.LogWarning($"Delete action: Content with ID {id} not found.");
                return NotFound();
            }

            return View(content);
        }

        // POST: Admin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var content = await _context.WebsiteContents.FindAsync(id);
                if (content == null)
                {
                    _logger.LogWarning($"DeleteConfirmed action: Content with ID {id} not found.");
                    return NotFound();
                }

                // Check if content is an image
                if (content.ContentType == "Image")
                {
                    // Check if content is already the placeholder
                    if (content.ContentValue == "/assets/placeholder.png")
                    {
                        // Delete the content from the database
                        _context.WebsiteContents.Remove(content);
                        _logger.LogInformation($"Content with ID {id} is a placeholder and has been deleted.");
                    }
                    else
                    {
                        // Replace the content value with the placeholder image path
                        string placeholderPath = "/assets/placeholder.png";
                        content.ContentValue = placeholderPath;
                        _logger.LogInformation($"Content with ID {id} replaced with placeholder.");
                    }
                }
                else
                {
                    // For non-image content, delete it
                    _context.WebsiteContents.Remove(content);
                    _logger.LogInformation($"Non-image content with ID {id} has been deleted.");
                }

                // Save changes to the database
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating content: {ex.Message} - StackTrace: {ex.StackTrace}");
                ModelState.AddModelError("", "An error occurred while updating the content.");
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
    }
}
