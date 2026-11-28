namespace IskolarTunes.ViewModels;

public class TrendingTrackViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ArtistName { get; set; }
    public int WeeklyPlays { get; set; }
    public int LikeCount { get; set; }
    public bool LikedByMe { get; set; }
}

public class LikeButtonViewModel
{
    public int TrackId { get; set; }
    public bool Liked { get; set; }
    public int Count { get; set; }
}