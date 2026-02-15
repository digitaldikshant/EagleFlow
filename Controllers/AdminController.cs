using EagleFlow.Data;
using EagleFlow.Models;
using EagleFlow.Models.ViewModels;
using EagleFlow.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EagleFlow.Controllers;

public class AdminController(
    ApplicationDbContext dbContext,
    IWebHostEnvironment environment,
    IDocumentNumberGenerator documentNumberGenerator,
    ILogger<AdminController> logger) : Controller
{
    private const long MaxFileSize = 10 * 1024 * 1024;

    [HttpGet]
    public async Task<IActionResult> Index(string? search)
    {
        var query = dbContext.Documents.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(d => d.DocumentNumber.Contains(search));
        }

        var documents = await query
            .OrderByDescending(d => d.UploadDate)
            .ToListAsync();

        ViewBag.Search = search;
        return View(documents);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(UploadDocumentViewModel model)
    {
        if (!ModelState.IsValid || model.File is null)
        {
            TempData["Error"] = "Please select a PDF file to upload.";
            return RedirectToAction(nameof(Index));
        }

        if (!model.File.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase)
            && !Path.GetExtension(model.File.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "Only PDF files are allowed.";
            return RedirectToAction(nameof(Index));
        }

        if (model.File.Length > MaxFileSize)
        {
            TempData["Error"] = "File size exceeds the 10 MB limit.";
            return RedirectToAction(nameof(Index));
        }

        var documentNumber = await GenerateUniqueDocumentNumberAsync();
        var storedFileName = $"{Guid.NewGuid():N}.pdf";
        var uploadsRoot = Path.Combine(environment.ContentRootPath, "App_Data", "Documents");
        Directory.CreateDirectory(uploadsRoot);
        var fullPath = Path.Combine(uploadsRoot, storedFileName);

        await using (var stream = System.IO.File.Create(fullPath))
        {
            await model.File.CopyToAsync(stream);
        }

        var document = new Document
        {
            DocumentNumber = documentNumber,
            OriginalFileName = Path.GetFileName(model.File.FileName),
            StoredFileName = storedFileName,
            FilePath = fullPath,
            FileSize = model.File.Length,
            UploadDate = DateTime.UtcNow,
            IsDeleted = false
        };

        dbContext.Documents.Add(document);
        await dbContext.SaveChangesAsync();

        TempData["Success"] = $"Document uploaded successfully. Number: {documentNumber}";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string documentNumber)
    {
        var document = await dbContext.Documents
            .FirstOrDefaultAsync(d => d.DocumentNumber == documentNumber && !d.IsDeleted);

        if (document is null)
        {
            TempData["Error"] = "Document not found.";
            return RedirectToAction(nameof(Index));
        }

        document.IsDeleted = true;
        await dbContext.SaveChangesAsync();

        TempData["Success"] = $"Document {documentNumber} marked as deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<string> GenerateUniqueDocumentNumberAsync()
    {
        for (var i = 0; i < 10; i++)
        {
            var candidate = documentNumberGenerator.Generate();
            var exists = await dbContext.Documents.AnyAsync(d => d.DocumentNumber == candidate);
            if (!exists)
            {
                return candidate;
            }
        }

        logger.LogError("Failed to generate a unique document number after retries.");
        throw new InvalidOperationException("Could not generate a unique document number.");
    }
}
