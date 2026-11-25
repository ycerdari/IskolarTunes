using System.ComponentModel.DataAnnotations;

namespace IskolarTunes.Models;

public class Track
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Genre { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public string AudioFilePath { get; set; } = string.Empty;

    public string? CoverImagePath { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public string UploaderId { get; set; } = string.Empty;
    public ApplicationUser? Uploader { get; set; }

    public ICollection<PlaylistTrack> PlaylistTracks { get; set; } = new List<PlaylistTrack>();
}