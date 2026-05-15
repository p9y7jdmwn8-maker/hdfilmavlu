using Microsoft.AspNetCore.Mvc;
using VideoSite.Services;
using VideoSite.ViewModels;

namespace VideoSite.Controllers;

public sealed class HomeController : Controller
{
    private readonly VideoCatalog _catalog;

    public HomeController(VideoCatalog catalog)
    {
        _catalog = catalog;
    }

    public IActionResult Index(string? category, string sort = "rating-desc", string? q = null)
    {
        return View(new HomeIndexViewModel
        {
            Categories = _catalog.GetCategories(),
            Videos = _catalog.GetVideos(category, sort, q),
            ActiveCategorySlug = category,
            Sort = sort,
            Query = q,
            PageTitle = "Ana Sayfa"
        });
    }

    [HttpGet("filmler")]
    public IActionResult Movies(string? category, string sort = "rating-desc", string? q = null)
    {
        return View("Listing", new HomeIndexViewModel
        {
            Categories = _catalog.GetCategories(),
            Videos = _catalog.GetVideos(category, sort, q, "movie"),
            ActiveCategorySlug = category,
            Sort = sort,
            Query = q,
            ContentType = "movie",
            PageTitle = "Filmler",
            PageDescription = "Sadece film arşivini yıl, IMDb, süre ve kategoriye göre keşfet."
        });
    }

    [HttpGet("diziler")]
    public IActionResult Series(string? category, string sort = "rating-desc", string? q = null)
    {
        return View("Listing", new HomeIndexViewModel
        {
            Categories = _catalog.GetCategories(),
            Videos = _catalog.GetVideos(category, sort, q, "series"),
            ActiveCategorySlug = category,
            Sort = sort,
            Query = q,
            ContentType = "series",
            PageTitle = "Diziler",
            PageDescription = "Sadece dizi içeriklerini kategori ve kalite ölçülerine göre sırala."
        });
    }

    [HttpGet("sitemap.xml")]
    public IActionResult Sitemap()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var urls = _catalog.GetVideos(sort: "newest")
            .Select(video => $"  <url><loc>{baseUrl}/Home/Watch/{video.Id}</loc><lastmod>{video.CreatedAt:yyyy-MM-dd}</lastmod></url>");
        var xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                  "<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">\n" +
                  $"  <url><loc>{baseUrl}/</loc></url>\n" +
                  $"  <url><loc>{baseUrl}/filmler</loc></url>\n" +
                  $"  <url><loc>{baseUrl}/diziler</loc></url>\n" +
                  string.Join("\n", urls) +
                  "\n</urlset>";

        return Content(xml, "application/xml");
    }

    public IActionResult Watch(Guid id)
    {
        var video = _catalog.GetVideo(id);
        return video is null ? NotFound() : View(video);
    }
}
