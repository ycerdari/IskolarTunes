using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace IskolarTunes.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [StringLength(10)]
    public string? College { get; set; }

    [StringLength(80)]
    public string? Program { get; set; }

    public DateOnly? Birthdate { get; set; }

    [StringLength(300)]
    public string? Bio { get; set; }

    [StringLength(80)]
    public string? Location { get; set; }

    [StringLength(200)]
    public string? Website { get; set; }

    [StringLength(30)]
    public string? Pronouns { get; set; }

    // Which fields other signed-in users may see. Default keeps your artist page working
    // exactly as today (name and college public); everything new starts private.
    public ProfileField PublicFields { get; set; } = ProfileField.FullName | ProfileField.College;

    public ICollection<UserSocialLink> SocialLinks { get; set; } = new List<UserSocialLink>();
}