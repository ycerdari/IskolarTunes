using System.ComponentModel.DataAnnotations;

namespace IskolarTunes.Models;

// One row per user per setting, e.g. Key = "theme", Value = "1".
public class UserSetting
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    [Required, StringLength(60)]
    public string Key { get; set; } = string.Empty;

    [StringLength(200)]
    public string Value { get; set; } = string.Empty;
}