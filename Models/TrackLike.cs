namespace IskolarTunes.Models;

public class TrackLike
{
    public int Id { get; set; }

    public int TrackId { get; set; }
    public Track? Track { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public DateTime LikedAt { get; set; } = DateTime.UtcNow;
}