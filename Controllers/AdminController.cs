using EagleFlow.Data;
using EagleFlow.Models;
using EagleFlow.Models.ViewModels;
using EagleFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EagleFlow.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController(
    ApplicationDbContext dbContext,
    IWebHostEnvironment environment,
    IDocumentNumberGenerator documentNumberGenerator,
    IEmailSender emailSender,
    ILogger<AdminController> logger) : Controller
{
    private const long MaxFileSize = 10 * 1024 * 1024;

    [HttpGet]
    public async Task<IActionResult> Index(string? search)
    {
        var query = dbContext.Documents.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(d => d.DocumentNumber.Contains(search.Trim()));
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

        if (model.File.Length == 0)
        {
            TempData["Error"] = "Uploaded file is empty.";
            return RedirectToAction(nameof(Index));
        }

        if (model.File.Length > MaxFileSize)
        {
            TempData["Error"] = "File size exceeds the 10 MB limit.";
            return RedirectToAction(nameof(Index));
        }

        if (!Path.GetExtension(model.File.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "Only PDF files are allowed.";
            return RedirectToAction(nameof(Index));
        }

        if (!await HasPdfSignatureAsync(model.File))
        {
            TempData["Error"] = "The uploaded file does not appear to be a valid PDF.";
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

        var notifyEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        if (!string.IsNullOrWhiteSpace(notifyEmail))
        {
            await emailSender.SendAsync(
                notifyEmail,
                "EagleFlow document upload success",
                $"Document uploaded successfully. Document Number: {documentNumber}\nOriginal Name: {document.OriginalFileName}");
        }

        TempData["Success"] = $"Document uploaded successfully. Number: {documentNumber}";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string documentNumber)
    {
        if (string.IsNullOrWhiteSpace(documentNumber))
        {
            TempData["Error"] = "Document number is required.";
            return RedirectToAction(nameof(Index));
        }

        var normalizedDocumentNumber = documentNumber.Trim();
        var document = await dbContext.Documents
            .FirstOrDefaultAsync(d => d.DocumentNumber == normalizedDocumentNumber && !d.IsDeleted);

        if (document is null)
        {
            TempData["Error"] = "Document not found.";
            return RedirectToAction(nameof(Index));
        }

        document.IsDeleted = true;
        await dbContext.SaveChangesAsync();

        TempData["Success"] = $"Document {normalizedDocumentNumber} marked as deleted.";
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

    private static async Task<bool> HasPdfSignatureAsync(IFormFile file)
    {
        const int signatureLength = 5;
        var buffer = new byte[signatureLength];

        await using var stream = file.OpenReadStream();
        var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, signatureLength));
        if (bytesRead < signatureLength)
        {
            return false;
        }

        return buffer[0] == 0x25 && // %
               buffer[1] == 0x50 && // P
               buffer[2] == 0x44 && // D
               buffer[3] == 0x46 && // F
               buffer[4] == 0x2D;   // -
    }
}
