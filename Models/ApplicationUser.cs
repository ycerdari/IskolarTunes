using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace IskolarTunes.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [StringLength(10)]
    public string? College { get; set; }
}
