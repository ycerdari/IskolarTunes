namespace IskolarTunes.ViewModels;

public class HomeTrackViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ArtistName { get; set; } = string.Empty;
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
    public List<HomeTrackViewModel> NewTracks { get; set; } = new();
    public List<HomeTrackViewModel> Trending { get; set; } = new();
    public List<HomeGenreViewModel> Genres { get; set; } = new();

    // Top trending track, or the newest one if nothing was played this week
    public HomeTrackViewModel? Spotlight => Trending.FirstOrDefault() ?? NewTracks.FirstOrDefault();

    // Everything the player can play, without duplicates
    public List<HomeTrackViewModel> Queue =>
        NewTracks.Concat(Trending).DistinctBy(t => t.Id).ToList();
}