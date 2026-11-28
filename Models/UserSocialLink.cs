using System.ComponentModel.DataAnnotations;

namespace IskolarTunes.Models;

public class UserSocialLink
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public SocialPlatform Platform { get; set; }

    // Only used when Platform is Other
    [StringLength(30)]
    public string? Label { get; set; }

    // A full https URL, or a handle for Discord and Telegram
    [Required, StringLength(300)]
    public string Value { get; set; } = string.Empty;

    // Private by default
    public bool IsPublic { get; set; }
}