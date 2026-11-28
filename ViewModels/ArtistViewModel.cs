namespace IskolarTunes.ViewModels;

public class ArtistTrackViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Genre { get; set; }
    public bool HasCover { get; set; }
    public int PlayCount { get; set; }
    public int LikeCount { get; set; }
}

public class ArtistViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? College { get; set; }
    public bool IsMe { get; set; }
    public int TotalPlays { get; set; }
    public int TotalLikes { get; set; }
    public List<ArtistTrackViewModel> Tracks { get; set; } = new();
}