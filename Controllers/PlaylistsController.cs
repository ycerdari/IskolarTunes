using IskolarTunes.Data;
using IskolarTunes.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IskolarTunes.Controllers;

[Authorize]
public class PlaylistsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public PlaylistsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // READ (list): public playlists plus my own private ones
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);

        var playlists = await _context.Playlists
            .Include(p => p.Owner)
            .Include(p => p.PlaylistTracks)
            .Where(p => p.IsPublic || p.OwnerId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return View(playlists);
    }

    // READ (one) with its tracks
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var playlist = await _context.Playlists
            .Include(p => p.Owner)
            .Include(p => p.PlaylistTracks.OrderBy(pt => pt.Position))
                .ThenInclude(pt => pt.Track!)
                    .ThenInclude(t => t.Uploader)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (playlist == null) return NotFound();

        var userId = _userManager.GetUserId(User);
        if (!playlist.IsPublic && playlist.OwnerId != userId) return NotFound();

        var trackIds = playlist.PlaylistTracks.Select(pt => pt.TrackId).ToList();

        ViewData["LikeCounts"] = await _context.TrackLikes
            .Where(l => trackIds.Contains(l.TrackId))
            .GroupBy(l => l.TrackId)
            .Select(g => new { TrackId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TrackId, x => x.Count);

        ViewData["LikedByMe"] = (await _context.TrackLikes
            .Where(l => trackIds.Contains(l.TrackId) && l.UserId == userId)
            .Select(l => l.TrackId)
            .ToListAsync()).ToHashSet();

        if (playlist.OwnerId == userId)
        {
            var existingIds = playlist.PlaylistTracks.Select(pt => pt.TrackId).ToList();
            var available = await _context.Tracks
                .Where(t => !existingIds.Contains(t.Id))
                .OrderBy(t => t.Title)
                .Select(t => new { t.Id, t.Title })
                .ToListAsync();

            ViewData["AvailableTracks"] = new SelectList(available, "Id", "Title");
        }

        return View(playlist);
    }

    // CREATE
    [Authorize]
    public IActionResult Create() => View(new PlaylistFormViewModel());

    [Authorize]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PlaylistFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var playlist = new Playlist
        {
            Name = vm.Name,
            Description = vm.Description,
            IsPublic = vm.IsPublic,
            OwnerId = _userManager.GetUserId(User)!
        };

        _context.Playlists.Add(playlist);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Playlist created.";
        return RedirectToAction(nameof(Details), new { id = playlist.Id });
    }

    // UPDATE
    [Authorize]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var playlist = await _context.Playlists.FindAsync(id);
        if (playlist == null) return NotFound();
        if (!IsOwner(playlist)) return Forbid();

        return View(new PlaylistFormViewModel
        {
            Id = playlist.Id,
            Name = playlist.Name,
            Description = playlist.Description,
            IsPublic = playlist.IsPublic
        });
    }

    [Authorize]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PlaylistFormViewModel vm)
    {
        if (id != vm.Id) return NotFound();

        var playlist = await _context.Playlists.FindAsync(id);
        if (playlist == null) return NotFound();
        if (!IsOwner(playlist)) return Forbid();

        if (!ModelState.IsValid) return View(vm);

        playlist.Name = vm.Name;
        playlist.Description = vm.Description;
        playlist.IsPublic = vm.IsPublic;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Playlist updated.";
        return RedirectToAction(nameof(Details), new { id = playlist.Id });
    }

    // DELETE
    [Authorize]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var playlist = await _context.Playlists
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (playlist == null) return NotFound();
        if (!IsOwner(playlist)) return Forbid();

        return View(playlist);
    }

    [Authorize]
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var playlist = await _context.Playlists.FindAsync(id);
        if (playlist == null) return RedirectToAction(nameof(Index));
        if (!IsOwner(playlist)) return Forbid();

        _context.Playlists.Remove(playlist); // PlaylistTracks are removed by cascade
        await _context.SaveChangesAsync();

        TempData["Success"] = "Playlist deleted.";
        return RedirectToAction(nameof(Index));
    }

    // ADD a track to a playlist
    [Authorize]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddTrack(int playlistId, int trackId)
    {
        var playlist = await _context.Playlists
            .Include(p => p.PlaylistTracks)
            .FirstOrDefaultAsync(p => p.Id == playlistId);

        if (playlist == null) return NotFound();
        if (!IsOwner(playlist)) return Forbid();

        if (!await _context.Tracks.AnyAsync(t => t.Id == trackId)) return NotFound();

        if (!playlist.PlaylistTracks.Any(pt => pt.TrackId == trackId))
        {
            var nextPosition = playlist.PlaylistTracks.Any()
                ? playlist.PlaylistTracks.Max(pt => pt.Position) + 1
                : 1;

            _context.PlaylistTracks.Add(new PlaylistTrack
            {
                PlaylistId = playlistId,
                TrackId = trackId,
                Position = nextPosition
            });
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Details), new { id = playlistId });
    }

    // REMOVE a track from a playlist
    [Authorize]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveTrack(int playlistId, int trackId)
    {
        var playlist = await _context.Playlists.FindAsync(playlistId);
        if (playlist == null) return NotFound();
        if (!IsOwner(playlist)) return Forbid();

        var link = await _context.PlaylistTracks
            .FirstOrDefaultAsync(pt => pt.PlaylistId == playlistId && pt.TrackId == trackId);

        if (link != null)
        {
            _context.PlaylistTracks.Remove(link);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Details), new { id = playlistId });
    }

    private bool IsOwner(Playlist playlist) => playlist.OwnerId == _userManager.GetUserId(User);
}