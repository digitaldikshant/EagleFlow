using EagleFlow.Data;
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
        var document = await dbContext.Documents
            .FirstOrDefaultAsync(d => d.DocumentNumber == documentNumber && !d.IsDeleted);

        if (document is null || !System.IO.File.Exists(document.FilePath))
        {
            TempData["Error"] = "Document not found.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.DocumentNumber = document.DocumentNumber;
        ViewBag.FileName = document.OriginalFileName;
        return File(System.IO.File.ReadAllBytes(document.FilePath), "application/pdf");
    }

    [HttpGet]
    public async Task<IActionResult> Download(string documentNumber)
    {
        var document = await dbContext.Documents
            .FirstOrDefaultAsync(d => d.DocumentNumber == documentNumber && !d.IsDeleted);

        if (document is null || !System.IO.File.Exists(document.FilePath))
        {
            TempData["Error"] = "Document not found.";
            return RedirectToAction(nameof(Index));
        }

        var fileBytes = await System.IO.File.ReadAllBytesAsync(document.FilePath);
        return File(fileBytes, "application/pdf", document.OriginalFileName);
    }
}
