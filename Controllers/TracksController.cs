using IskolarTunes.Data;
using IskolarTunes.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IskolarTunes.Controllers;

public class TracksController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;

    private static readonly string[] AudioExtensions = { ".mp3", ".wav", ".ogg", ".m4a" };
    private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private const long MaxAudioBytes = 20 * 1024 * 1024;
    private const long MaxImageBytes = 3 * 1024 * 1024;

    public TracksController(ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment env)
    {
        _context = context;
        _userManager = userManager;
        _env = env;
    }

    // READ (list + search)
    public async Task<IActionResult> Index(string? search)
    {
        var query = _context.Tracks.Include(t => t.Uploader).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(t =>
                t.Title.Contains(search) ||
                (t.Genre != null && t.Genre.Contains(search)) ||
                t.Uploader!.FullName.Contains(search));
        }

        ViewData["Search"] = search;
        var tracks = await query.OrderByDescending(t => t.UploadedAt).ToListAsync();
        return View(tracks);
    }

    // READ (one)
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var track = await _context.Tracks
            .Include(t => t.Uploader)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (track == null) return NotFound();
        return View(track);
    }

    // CREATE
    [Authorize]
    public IActionResult Create() => View(new TrackFormViewModel());

    [Authorize]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TrackFormViewModel vm)
    {
        if (vm.AudioFile == null)
            ModelState.AddModelError(nameof(vm.AudioFile), "Please choose an audio file.");
        else
            ValidateFile(vm.AudioFile, nameof(vm.AudioFile), AudioExtensions, MaxAudioBytes, "audio");

        if (vm.CoverImage != null)
            ValidateFile(vm.CoverImage, nameof(vm.CoverImage), ImageExtensions, MaxImageBytes, "image");

        if (!ModelState.IsValid) return View(vm);

        var track = new Track
        {
            Title = vm.Title,
            Genre = vm.Genre,
            Description = vm.Description,
            UploaderId = _userManager.GetUserId(User)!,
            AudioFilePath = await SaveFileAsync(vm.AudioFile!, "audio")
        };

        if (vm.CoverImage != null)
            track.CoverImagePath = await SaveFileAsync(vm.CoverImage, "covers");

        _context.Tracks.Add(track);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Track uploaded.";
        return RedirectToAction(nameof(Details), new { id = track.Id });
    }

    // UPDATE
    [Authorize]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var track = await _context.Tracks.FindAsync(id);
        if (track == null) return NotFound();
        if (!IsOwner(track)) return Forbid();

        return View(new TrackFormViewModel
        {
            Id = track.Id,
            Title = track.Title,
            Genre = track.Genre,
            Description = track.Description,
            ExistingAudioPath = track.AudioFilePath,
            ExistingCoverPath = track.CoverImagePath
        });
    }

    [Authorize]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TrackFormViewModel vm)
    {
        if (id != vm.Id) return NotFound();

        var track = await _context.Tracks.FindAsync(id);
        if (track == null) return NotFound();
        if (!IsOwner(track)) return Forbid();

        if (vm.AudioFile != null)
            ValidateFile(vm.AudioFile, nameof(vm.AudioFile), AudioExtensions, MaxAudioBytes, "audio");
        if (vm.CoverImage != null)
            ValidateFile(vm.CoverImage, nameof(vm.CoverImage), ImageExtensions, MaxImageBytes, "image");

        if (!ModelState.IsValid)
        {
            vm.ExistingAudioPath = track.AudioFilePath;
            vm.ExistingCoverPath = track.CoverImagePath;
            return View(vm);
        }

        track.Title = vm.Title;
        track.Genre = vm.Genre;
        track.Description = vm.Description;

        if (vm.AudioFile != null)
        {
            DeleteFile(track.AudioFilePath);
            track.AudioFilePath = await SaveFileAsync(vm.AudioFile, "audio");
        }

        if (vm.CoverImage != null)
        {
            DeleteFile(track.CoverImagePath);
            track.CoverImagePath = await SaveFileAsync(vm.CoverImage, "covers");
        }

        await _context.SaveChangesAsync();

        TempData["Success"] = "Track updated.";
        return RedirectToAction(nameof(Details), new { id = track.Id });
    }

    // DELETE
    [Authorize]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var track = await _context.Tracks
            .Include(t => t.Uploader)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (track == null) return NotFound();
        if (!IsOwner(track)) return Forbid();

        return View(track);
    }

    [Authorize]
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var track = await _context.Tracks.FindAsync(id);
        if (track == null) return RedirectToAction(nameof(Index));
        if (!IsOwner(track)) return Forbid();

        DeleteFile(track.AudioFilePath);
        DeleteFile(track.CoverImagePath);

        _context.Tracks.Remove(track);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Track deleted.";
        return RedirectToAction(nameof(Index));
    }

    // Helpers
    private bool IsOwner(Track track) => track.UploaderId == _userManager.GetUserId(User);

    private void ValidateFile(IFormFile file, string field, string[] allowed, long maxBytes, string kind)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowed.Contains(ext))
            ModelState.AddModelError(field, $"Allowed {kind} types: {string.Join(", ", allowed)}");
        else if (file.Length == 0)
            ModelState.AddModelError(field, "The file is empty.");
        else if (file.Length > maxBytes)
            ModelState.AddModelError(field, $"The {kind} file must be {maxBytes / (1024 * 1024)} MB or smaller.");
    }

    private async Task<string> SaveFileAsync(IFormFile file, string folder)
    {
        var folderPath = Path.Combine(_env.WebRootPath, "uploads", folder);
        Directory.CreateDirectory(folderPath);

        var fileName = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName).ToLowerInvariant()}";
        var fullPath = Path.Combine(folderPath, fileName);

        using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/{folder}/{fileName}";
    }

    private void DeleteFile(string? webPath)
    {
        if (string.IsNullOrEmpty(webPath)) return;

        var fullPath = Path.Combine(_env.WebRootPath,
            webPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

        if (System.IO.File.Exists(fullPath))
            System.IO.File.Delete(fullPath);
    }
}