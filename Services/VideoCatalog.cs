using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using VideoSite.Models;

namespace VideoSite.Services;

public sealed partial class VideoCatalog
{
    private readonly IWebHostEnvironment _environment;
    private readonly string _databasePath;

    public VideoCatalog(IWebHostEnvironment environment)
    {
        _environment = environment;
        var dataDirectory = Path.Combine(environment.ContentRootPath, "App_Data");
        _databasePath = Path.Combine(dataDirectory, "hdfilmavlu.db");

        Directory.CreateDirectory(dataDirectory);
        Directory.CreateDirectory(Path.Combine(environment.WebRootPath, "uploads", "videos"));
        Directory.CreateDirectory(Path.Combine(environment.WebRootPath, "uploads", "posters"));

        EnsureDatabase();
    }

    public IReadOnlyList<Category> GetCategories()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "select Slug, Name from Categories order by Name";

        using var reader = command.ExecuteReader();
        var categories = new List<Category>();
        while (reader.Read())
        {
            categories.Add(new Category
            {
                Slug = reader.GetString(0),
                Name = reader.GetString(1)
            });
        }

        return categories;
    }

    public IReadOnlyList<VideoItem> GetVideos(string? categorySlug = null, string sort = "rating-desc", string? query = null, string? contentType = null)
    {
        var videos = ReadAllVideos().AsEnumerable();

        if (!string.IsNullOrWhiteSpace(contentType))
        {
            videos = videos.Where(video => video.ContentType == contentType);
        }

        if (!string.IsNullOrWhiteSpace(categorySlug))
        {
            videos = videos.Where(video => video.CategorySlug == categorySlug);
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            var normalizedQuery = query.Trim();
            videos = videos.Where(video =>
                video.Title.Contains(normalizedQuery, StringComparison.CurrentCultureIgnoreCase) ||
                video.Description.Contains(normalizedQuery, StringComparison.CurrentCultureIgnoreCase) ||
                video.CategorySlug.Contains(normalizedQuery, StringComparison.CurrentCultureIgnoreCase));
        }

        videos = sort switch
        {
            "rating-asc" => videos.OrderBy(video => video.ImdbRating),
            "year-desc" => videos.OrderByDescending(video => video.Year),
            "year-asc" => videos.OrderBy(video => video.Year),
            "duration-desc" => videos.OrderByDescending(video => video.DurationMinutes),
            "duration-asc" => videos.OrderBy(video => video.DurationMinutes),
            "title-asc" => videos.OrderBy(video => video.Title),
            "title-desc" => videos.OrderByDescending(video => video.Title),
            "newest" => videos.OrderByDescending(video => video.CreatedAt),
            "oldest" => videos.OrderBy(video => video.CreatedAt),
            _ => videos.OrderByDescending(video => video.ImdbRating)
        };

        return videos.ToList();
    }

    public VideoItem? GetVideo(Guid id) => ReadAllVideos().FirstOrDefault(video => video.Id == id);

    public void AddCategory(string name)
    {
        var normalized = name.Trim();
        if (normalized.Length == 0)
        {
            return;
        }

        var slug = CreateUniqueSlug(normalized, GetCategories().Select(category => category.Slug));

        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "insert into Categories (Slug, Name) values ($slug, $name)";
        command.Parameters.AddWithValue("$slug", slug);
        command.Parameters.AddWithValue("$name", normalized);
        command.ExecuteNonQuery();
    }

    public async Task<VideoItem> AddVideoAsync(string title, string description, string contentType, string categorySlug, int durationMinutes, int year, double imdbRating, IFormFile videoFile, IFormFile? posterFile)
    {
        var id = Guid.NewGuid();
        var videoExtension = Path.GetExtension(videoFile.FileName);
        var videoFileName = $"{id}{videoExtension}";
        var videoPath = Path.Combine(_environment.WebRootPath, "uploads", "videos", videoFileName);

        await using (var stream = File.Create(videoPath))
        {
            await videoFile.CopyToAsync(stream);
        }

        var posterFileName = "";
        if (posterFile is { Length: > 0 })
        {
            var posterExtension = Path.GetExtension(posterFile.FileName);
            posterFileName = $"{id}{posterExtension}";
            var posterPath = Path.Combine(_environment.WebRootPath, "uploads", "posters", posterFileName);

            await using var stream = File.Create(posterPath);
            await posterFile.CopyToAsync(stream);
        }

        var video = new VideoItem
        {
            Id = id,
            Title = title.Trim(),
            Description = description.Trim(),
            ContentType = NormalizeContentType(contentType),
            CategorySlug = categorySlug,
            DurationMinutes = durationMinutes,
            Year = year > 1900 ? year : DateTime.UtcNow.Year,
            ImdbRating = Math.Clamp(imdbRating, 0, 10),
            FileName = videoFileName,
            PosterFileName = posterFileName,
            CreatedAt = DateTime.UtcNow
        };

        InsertVideo(video);
        return video;
    }

    private void EnsureDatabase()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            create table if not exists Categories (
                Slug text primary key,
                Name text not null
            );

            create table if not exists Videos (
                Id text primary key,
                Title text not null,
                Description text not null,
                ContentType text not null default 'movie',
                CategorySlug text not null,
                FileName text not null,
                PosterFileName text not null,
                Year integer not null,
                ImdbRating real not null,
                DurationMinutes integer not null,
                CreatedAt text not null
            );
            """;
        command.ExecuteNonQuery();
        EnsureVideoContentTypeColumn(connection);

        if (GetTableCount(connection, "Categories") == 0)
        {
            foreach (var category in CreateDemoCategories())
            {
                InsertCategory(connection, category);
            }
        }

        if (GetTableCount(connection, "Videos") == 0)
        {
            foreach (var video in CreateDemoVideos())
            {
                InsertVideo(connection, video);
            }
        }
    }

    private List<VideoItem> ReadAllVideos()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            select Id, Title, Description, ContentType, CategorySlug, FileName, PosterFileName, Year, ImdbRating, DurationMinutes, CreatedAt
            from Videos
            """;

        using var reader = command.ExecuteReader();
        var videos = new List<VideoItem>();
        while (reader.Read())
        {
            videos.Add(new VideoItem
            {
                Id = Guid.Parse(reader.GetString(0)),
                Title = reader.GetString(1),
                Description = reader.GetString(2),
                ContentType = reader.GetString(3),
                CategorySlug = reader.GetString(4),
                FileName = reader.GetString(5),
                PosterFileName = reader.GetString(6),
                Year = reader.GetInt32(7),
                ImdbRating = reader.GetDouble(8),
                DurationMinutes = reader.GetInt32(9),
                CreatedAt = DateTime.Parse(reader.GetString(10), null, System.Globalization.DateTimeStyles.RoundtripKind)
            });
        }

        return videos;
    }

    private SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection($"Data Source={_databasePath}");
        connection.Open();
        return connection;
    }

    private static int GetTableCount(SqliteConnection connection, string tableName)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"select count(*) from {tableName}";
        return Convert.ToInt32(command.ExecuteScalar());
    }

    private static void EnsureVideoContentTypeColumn(SqliteConnection connection)
    {
        using (var checkCommand = connection.CreateCommand())
        {
            checkCommand.CommandText = "pragma table_info(Videos)";
            using var reader = checkCommand.ExecuteReader();
            var hasContentType = false;
            while (reader.Read())
            {
                if (string.Equals(reader.GetString(1), "ContentType", StringComparison.OrdinalIgnoreCase))
                {
                    hasContentType = true;
                    break;
                }
            }

            if (!hasContentType)
            {
                using var alterCommand = connection.CreateCommand();
                alterCommand.CommandText = "alter table Videos add column ContentType text not null default 'movie'";
                alterCommand.ExecuteNonQuery();
            }
        }

        using var updateCommand = connection.CreateCommand();
        updateCommand.CommandText = """
            update Videos
            set ContentType = case
                when Title in ('Dünya Notları', 'Dunya Notlari', 'Kırık Saatler', 'Kirik Saatler', 'Uzak Ev') then 'series'
                else 'movie'
            end
            where ContentType is null or ContentType = '' or ContentType = 'movie'
            """;
        updateCommand.ExecuteNonQuery();
    }

    private static void InsertCategory(SqliteConnection connection, Category category)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "insert or ignore into Categories (Slug, Name) values ($slug, $name)";
        command.Parameters.AddWithValue("$slug", category.Slug);
        command.Parameters.AddWithValue("$name", category.Name);
        command.ExecuteNonQuery();
    }

    private void InsertVideo(VideoItem video)
    {
        using var connection = OpenConnection();
        InsertVideo(connection, video);
    }

    private static void InsertVideo(SqliteConnection connection, VideoItem video)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
            insert or replace into Videos
                (Id, Title, Description, ContentType, CategorySlug, FileName, PosterFileName, Year, ImdbRating, DurationMinutes, CreatedAt)
            values
                ($id, $title, $description, $contentType, $categorySlug, $fileName, $posterFileName, $year, $imdbRating, $durationMinutes, $createdAt)
            """;
        command.Parameters.AddWithValue("$id", video.Id.ToString());
        command.Parameters.AddWithValue("$title", video.Title);
        command.Parameters.AddWithValue("$description", video.Description);
        command.Parameters.AddWithValue("$contentType", NormalizeContentType(video.ContentType));
        command.Parameters.AddWithValue("$categorySlug", video.CategorySlug);
        command.Parameters.AddWithValue("$fileName", video.FileName);
        command.Parameters.AddWithValue("$posterFileName", video.PosterFileName);
        command.Parameters.AddWithValue("$year", video.Year);
        command.Parameters.AddWithValue("$imdbRating", video.ImdbRating);
        command.Parameters.AddWithValue("$durationMinutes", video.DurationMinutes);
        command.Parameters.AddWithValue("$createdAt", video.CreatedAt.ToString("O"));
        command.ExecuteNonQuery();
    }

    private static List<Category> CreateDemoCategories()
    {
        return
        [
            new() { Name = "Aksiyon", Slug = "aksiyon" },
            new() { Name = "Dram", Slug = "dram" },
            new() { Name = "Belgesel", Slug = "belgesel" }
        ];
    }

    private static List<VideoItem> CreateDemoVideos()
    {
        var now = DateTime.UtcNow;

        return
        [
            new()
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Title = "Gece Operasyonu",
                Description = "Aksiyon kategorisi için örnek film kartı.",
                ContentType = "movie",
                CategorySlug = "aksiyon",
                Year = 2025,
                ImdbRating = 8.8,
                DurationMinutes = 60,
                CreatedAt = now.AddMinutes(-10)
            },
            new()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Title = "Sessiz Şehir",
                Description = "Dram kategorisi için örnek film kartı.",
                ContentType = "movie",
                CategorySlug = "dram",
                Year = 2022,
                ImdbRating = 8.1,
                DurationMinutes = 45,
                CreatedAt = now.AddMinutes(-20)
            },
            new()
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Title = "Derin Mavi",
                Description = "Belgesel kategorisi için örnek video kartı.",
                ContentType = "movie",
                CategorySlug = "belgesel",
                Year = 2024,
                ImdbRating = 7.6,
                DurationMinutes = 30,
                CreatedAt = now.AddMinutes(-30)
            },
            new()
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Title = "Son Takip",
                Description = "Aksiyon kategorisi için ikinci örnek kart.",
                ContentType = "movie",
                CategorySlug = "aksiyon",
                Year = 2021,
                ImdbRating = 8.4,
                DurationMinutes = 50,
                CreatedAt = now.AddMinutes(-40)
            },
            new()
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                Title = "Kırık Saatler",
                Description = "Dram kategorisi için ikinci örnek kart.",
                ContentType = "series",
                CategorySlug = "dram",
                Year = 2020,
                ImdbRating = 7.9,
                DurationMinutes = 55,
                CreatedAt = now.AddMinutes(-50)
            },
            new()
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                Title = "Dünya Notları",
                Description = "Belgesel kategorisi için ikinci örnek kart.",
                ContentType = "series",
                CategorySlug = "belgesel",
                Year = 2023,
                ImdbRating = 8.6,
                DurationMinutes = 42,
                CreatedAt = now.AddMinutes(-60)
            },
            new()
            {
                Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                Title = "Hızlı Plan",
                Description = "Aksiyon kategorisi için üçüncü örnek kart.",
                ContentType = "movie",
                CategorySlug = "aksiyon",
                Year = 2019,
                ImdbRating = 7.3,
                DurationMinutes = 35,
                CreatedAt = now.AddMinutes(-70)
            },
            new()
            {
                Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                Title = "Uzak Ev",
                Description = "Dram kategorisi için üçüncü örnek kart.",
                ContentType = "series",
                CategorySlug = "dram",
                Year = 2018,
                ImdbRating = 8.2,
                DurationMinutes = 58,
                CreatedAt = now.AddMinutes(-80)
            }
        ];
    }

    private static string CreateUniqueSlug(string value, IEnumerable<string> existingSlugs)
    {
        var slug = SlugInvalidChars().Replace(value.ToLowerInvariant(), "-").Trim('-');
        slug = string.IsNullOrWhiteSpace(slug) ? "kategori" : slug;

        var existing = existingSlugs.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var candidate = slug;
        var index = 2;
        while (existing.Contains(candidate))
        {
            candidate = $"{slug}-{index++}";
        }

        return candidate;
    }

    private static string NormalizeContentType(string contentType)
    {
        return string.Equals(contentType, "series", StringComparison.OrdinalIgnoreCase) ? "series" : "movie";
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex SlugInvalidChars();
}
