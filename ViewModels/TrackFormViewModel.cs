using System.ComponentModel.DataAnnotations;

namespace IskolarTunes.ViewModels;

public class TrackFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Genre { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [Display(Name = "Audio file (mp3, wav, ogg, m4a)")]
    public IFormFile? AudioFile { get; set; }

    [Display(Name = "Cover image (optional)")]
    public IFormFile? CoverImage { get; set; }

    public string? ExistingAudioPath { get; set; }
    public string? ExistingCoverPath { get; set; }
}