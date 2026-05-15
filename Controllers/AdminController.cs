using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VideoSite.Services;

namespace VideoSite.Controllers;

[Authorize(Roles = "Admin")]
public sealed class AdminController : Controller
{
    private readonly VideoCatalog _catalog;

    public AdminController(VideoCatalog catalog)
    {
        _catalog = catalog;
    }

    public IActionResult Index()
    {
        ViewBag.Categories = _catalog.GetCategories().Count;
        ViewBag.Videos = _catalog.GetVideos().Count;
        return View();
    }

    public IActionResult Upload()
    {
        ViewBag.Categories = _catalog.GetCategories();
        return View();
    }

    [HttpPost]
    [RequestSizeLimit(5L * 1024 * 1024 * 1024)]
    public async Task<IActionResult> Upload(string title, string description, string contentType, string categorySlug, int durationMinutes, int year, double imdbRating, IFormFile videoFile, IFormFile? posterFile)
    {
        if (videoFile is null || videoFile.Length == 0)
        {
            ModelState.AddModelError(nameof(videoFile), "Video dosyası seçmelisiniz.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = _catalog.GetCategories();
            return View();
        }

        var video = await _catalog.AddVideoAsync(title, description, contentType, categorySlug, durationMinutes, year, imdbRating, videoFile!, posterFile);
        return RedirectToAction("Watch", "Home", new { id = video.Id });
    }

    public IActionResult Categories()
    {
        return View(_catalog.GetCategories());
    }

    [HttpPost]
    public IActionResult Categories(string name)
    {
        _catalog.AddCategory(name);
        return RedirectToAction(nameof(Categories));
    }
}
