namespace VideoSite.Models;

public sealed class VideoItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string ContentType { get; set; } = "movie";
    public string CategorySlug { get; set; } = "";
    public string FileName { get; set; } = "";
    public string PosterFileName { get; set; } = "";
    public int Year { get; set; }
    public double ImdbRating { get; set; }
    public int DurationMinutes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
