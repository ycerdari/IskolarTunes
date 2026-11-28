using IskolarTunes.Data;
using IskolarTunes.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IskolarTunes.Controllers;

[Authorize]
public class ArtistsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ArtistsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("artists/{id}")]
    public async Task<IActionResult> Details(string id)
    {
        var artist = await _context.Users.AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new { u.Id, u.FullName, u.College })
            .FirstOrDefaultAsync();

        if (artist == null) return NotFound();

        var tracks = await _context.Tracks.AsNoTracking()
            .Where(t => t.UploaderId == id)
            .OrderByDescending(t => t.UploadedAt)
            .Select(t => new ArtistTrackViewModel
            {
                Id = t.Id,
                Title = t.Title,
                Genre = t.Genre,
                HasCover = t.CoverImagePath != null,
                PlayCount = t.PlayCount,
                LikeCount = t.Likes.Count()
            })
            .ToListAsync();

        var vm = new ArtistViewModel
        {
            Id = artist.Id,
            Name = artist.FullName ?? string.Empty,
            College = artist.College,
            IsMe = artist.Id == User.FindFirstValue(ClaimTypes.NameIdentifier),
            Tracks = tracks,
            TotalPlays = tracks.Sum(t => t.PlayCount),
            TotalLikes = tracks.Sum(t => t.LikeCount)
        };

        return View(vm);
    }
}