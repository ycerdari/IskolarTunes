namespace IskolarTunes.ViewModels;

public class HomeTrackViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ArtistId { get; set; } = string.Empty;
    public string ArtistName { get; set; } = string.Empty;
    public string? College { get; set; }
    public string? Genre { get; set; }
    public bool HasCover { get; set; }
    public int WeeklyPlays { get; set; }
    public int LikeCount { get; set; }
    public bool LikedByMe { get; set; }
}

public class HomeGenreViewModel
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class HomeViewModel
{
    public List<HomeTrackViewModel> Latest { get; set; } = new();     // newest overall
    public List<HomeTrackViewModel> Shelf { get; set; } = new();      // newest matching the filters
    public List<HomeTrackViewModel> Trending { get; set; } = new();
    public List<HomeGenreViewModel> Genres { get; set; } = new();

    public string? College { get; set; }
    public string? Genre { get; set; }

    public bool Filtered => College != null || Genre != null;

    public string ShelfTitle => Filtered
        ? string.Join(" · ", new[] { College, Genre }.Where(x => x != null)) + " songs"
        : "New from PUP artists";

    // Top trending track, or the newest one if nothing was played this week
    public HomeTrackViewModel? Spotlight => Trending.FirstOrDefault() ?? Latest.FirstOrDefault();

    // Everything the player can play, without duplicates
    public List<HomeTrackViewModel> Queue =>
        Latest.Concat(Trending).Concat(Shelf).DistinctBy(t => t.Id).ToList();
}