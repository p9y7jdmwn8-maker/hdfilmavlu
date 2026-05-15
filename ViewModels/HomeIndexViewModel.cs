using VideoSite.Models;

namespace VideoSite.ViewModels;

public sealed class HomeIndexViewModel
{
    public IReadOnlyList<Category> Categories { get; init; } = [];
    public IReadOnlyList<VideoItem> Videos { get; init; } = [];
    public string? ActiveCategorySlug { get; init; }
    public string Sort { get; init; } = "rating-desc";
    public string? Query { get; init; }
    public string? ContentType { get; init; }
    public string PageTitle { get; init; } = "Ana Sayfa";
    public string PageDescription { get; init; } = "";
}
