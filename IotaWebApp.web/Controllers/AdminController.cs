using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IotaWebApp.Data;
using IotaWebApp.Models;
using System.Linq;

namespace IotaWebApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly WebsiteCMSDbContext _context;
        private readonly string _uploadsFolder = "uploads";

        public AdminController(WebsiteCMSDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                var contents = await _context.WebsiteContents.ToListAsync();
                return View(contents);
            }
            catch (Exception)
            {
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
            ModelState.Remove("file");
            ModelState.Remove("ContentValue");

            ValidateContent(content, file, isEdit: false);

            if (ModelState.IsValid)
            {
                try
                {
                    if (content.ContentType == ContentType.Image.ToString() && file != null)
                    {
                        content.ContentValue = await SaveFileAsync(file);
                    }

                    _context.WebsiteContents.Add(content);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the content.");
                }
            }
            else
            {
                // NOTE: ADD PROPER EXCEPTION HANDLING HERE
            }

            return View(content);
        }

        // GET: Admin/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var content = await _context.WebsiteContents.FindAsync(id);
            if (content == null)
            {
                return NotFound();
            }

            return View(content);
        }

        // POST: Admin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, WebsiteContent content, IFormFile file)
        {
            if (id != content.Id)
            {
                return NotFound();
            }

            var existingContent = await _context.WebsiteContents.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (existingContent == null)
            {
                return NotFound();
            }

            ModelState.Remove("file");

            ValidateContent(content, file, isEdit: true, existingContent);

            if (ModelState.IsValid)
            {
                try
                {
                    if (content.ContentType == ContentType.Image.ToString() && file != null)
                    {
                        content.ContentValue = await SaveFileAsync(file);
                    }
                    else if (content.ContentType == ContentType.Image.ToString() && string.IsNullOrEmpty(content.ContentValue))
                    {
                        content.ContentValue = existingContent.ContentValue;
                    }

                    _context.Update(content);
                    await _context.SaveChangesAsync();

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
                catch (Exception)
                {
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the content.");
                }
            }
            else
            {
                // NOTE: ADD PROPER EXCEPTION HANDLING HERE
            }

            return View(content);
        }

        // GET: Admin/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var content = await _context.WebsiteContents.FindAsync(id);
            if (content == null)
            {
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
                    return NotFound();
                }

                if (content.ContentType == ContentType.Image.ToString())
                {
                    if (content.ContentValue == "/assets/placeholder.png")
                    {
                        _context.WebsiteContents.Remove(content);
                    }
                    else
                    {
                        content.ContentValue = "/assets/placeholder.png";
                        _context.WebsiteContents.Update(content);
                    }
                }
                else
                {
                    _context.WebsiteContents.Remove(content);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the content.");
                var content = await _context.WebsiteContents.FindAsync(id);
                return View("Delete", content);
            }
        }

        #region Helper Methods

        /// <summary>
        /// Validates the content based on its type and handles file uploads if necessary.
        /// </summary>
        /// <param name="content">The WebsiteContent model.</param>
        /// <param name="file">The uploaded file.</param>
        /// <param name="isEdit">Indicates if the action is an edit operation.</param>
        /// <param name="existingContent">The existing content from the database (only for edit operations).</param>
        private void ValidateContent(WebsiteContent content, IFormFile file, bool isEdit, WebsiteContent existingContent = null)
        {
            if (content.ContentType == ContentType.Image.ToString())
            {
                if (file != null && file.Length > 0)
                {
                    // File will be handled in the calling method
                }
                else if (isEdit && existingContent != null && !string.IsNullOrEmpty(existingContent.ContentValue))
                {
                    // Keep existing ContentValue
                    content.ContentValue = existingContent.ContentValue;
                }
                else
                {
                    ModelState.AddModelError("file", "An image file is required for Image content type.");
                }
            }
            else if (content.ContentType == ContentType.Text.ToString())
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
        }

        /// <summary>
        /// Checks if a WebsiteContent exists with the given ID.
        /// </summary>
        /// <param name="id">The content ID.</param>
        /// <returns>True if exists; otherwise, false.</returns>
        private bool WebsiteContentExists(int id)
        {
            return _context.WebsiteContents.Any(e => e.Id == id);
        }

        /// <summary>
        /// Saves the uploaded file to the designated uploads folder.
        /// </summary>
        /// <param name="file">The uploaded file.</param>
        /// <returns>The relative path to the saved file.</returns>
        private async Task<string> SaveFileAsync(IFormFile file)
        {
            try
            {
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", _uploadsFolder);
                Directory.CreateDirectory(uploadPath); // Ensure the uploads folder exists

                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                var filePath = Path.Combine(uploadPath, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return $"/{_uploadsFolder}/{uniqueFileName}"; // Return the relative path for storage in the database
            }
            catch (Exception)
            {
                ModelState.AddModelError("file", "An error occurred while uploading the file.");
                return null;
            }
        }

        /// <summary>
        /// Replaces dependent carousel items with a placeholder image.
        /// </summary>
        /// <param name="content">The content to replace.</param>
        private void ReplaceWithPlaceholder(WebsiteContent content)
        {
            try
            {
                string placeholderPath = "/assets/placeholder.png";

                var dependentItems = _context.WebsiteContents
                    .Where(c => c.ContentKey.StartsWith("Carousel") && c.ContentValue == content.ContentValue)
                    .ToList();

                foreach (var item in dependentItems)
                {
                    item.ContentValue = placeholderPath;
                }

                _context.SaveChanges();
            }
            catch (Exception){}
        }

        #endregion
    }

    /// <summary>
    /// Enum representing the types of content.
    /// </summary>
    public enum ContentType
    {
        Text,
        Image
    }
}
