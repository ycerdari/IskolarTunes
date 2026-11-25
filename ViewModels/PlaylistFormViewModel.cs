using System.ComponentModel.DataAnnotations;

namespace IskolarTunes.ViewModels;

public class PlaylistFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Display(Name = "Public playlist")]
    public bool IsPublic { get; set; } = true;
}