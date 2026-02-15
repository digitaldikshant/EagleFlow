using EagleFlow.Data;
using EagleFlow.Models;
using EagleFlow.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EagleFlow.Controllers;

public class PublicController(ApplicationDbContext dbContext) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View(new DocumentLookupViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Search(DocumentLookupViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", model);
        }

        return RedirectToAction(nameof(ViewDocument), new { documentNumber = model.DocumentNumber.Trim() });
    }

    [HttpGet]
    public async Task<IActionResult> ViewDocument(string documentNumber)
    {
        var document = await FindActiveDocumentAsync(documentNumber);

        if (document is null)
        {
            TempData["Error"] = "Document not found.";
            return RedirectToAction(nameof(Index));
        }

        return PhysicalFile(document.FilePath, "application/pdf");
    }

    [HttpGet]
    public async Task<IActionResult> Download(string documentNumber)
    {
        var document = await FindActiveDocumentAsync(documentNumber);

        if (document is null)
        {
            TempData["Error"] = "Document not found.";
            return RedirectToAction(nameof(Index));
        }

        return PhysicalFile(document.FilePath, "application/pdf", document.OriginalFileName);
    }

    private async Task<Document?> FindActiveDocumentAsync(string documentNumber)
    {
        if (string.IsNullOrWhiteSpace(documentNumber))
        {
            return null;
        }

        var normalizedDocumentNumber = documentNumber.Trim();
        var document = await dbContext.Documents
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.DocumentNumber == normalizedDocumentNumber && !d.IsDeleted);

        if (document is null || !System.IO.File.Exists(document.FilePath))
        {
            return null;
        }

        return document;
    }
}
