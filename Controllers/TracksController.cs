using IskolarTunes.Data;
using IskolarTunes.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IskolarTunes.Controllers;

[Authorize]
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
    public IActionResult Create() => View(new TrackFormViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    [RequestSizeLimit(30_000_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 30_000_000)]
    public async Task<IActionResult> Create(TrackFormViewModel vm)
    {
        if (vm.AudioFile == null)
            ModelState.AddModelError(nameof(vm.AudioFile), "Please choose an audio file.");
        else
            await ValidateFileAsync(vm.AudioFile, nameof(vm.AudioFile), AudioExtensions, MaxAudioBytes, "audio");

        if (vm.CoverImage != null)
            await ValidateFileAsync(vm.CoverImage, nameof(vm.CoverImage), ImageExtensions, MaxImageBytes, "image");

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

    [HttpPost, ValidateAntiForgeryToken]
    [RequestSizeLimit(30_000_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 30_000_000)]
    public async Task<IActionResult> Edit(int id, TrackFormViewModel vm)
    {
        if (id != vm.Id) return NotFound();

        var track = await _context.Tracks.FindAsync(id);
        if (track == null) return NotFound();
        if (!IsOwner(track)) return Forbid();

        if (vm.AudioFile != null)
            await ValidateFileAsync(vm.AudioFile, nameof(vm.AudioFile), AudioExtensions, MaxAudioBytes, "audio");
        if (vm.CoverImage != null)
            await ValidateFileAsync(vm.CoverImage, nameof(vm.CoverImage), ImageExtensions, MaxImageBytes, "image");

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
            DeleteFile(track.AudioFilePath, "audio");
            track.AudioFilePath = await SaveFileAsync(vm.AudioFile, "audio");
        }

        if (vm.CoverImage != null)
        {
            DeleteFile(track.CoverImagePath, "covers");
            track.CoverImagePath = await SaveFileAsync(vm.CoverImage, "covers");
        }

        await _context.SaveChangesAsync();

        TempData["Success"] = "Track updated.";
        return RedirectToAction(nameof(Details), new { id = track.Id });
    }

    // DELETE
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

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var track = await _context.Tracks.FindAsync(id);
        if (track == null) return RedirectToAction(nameof(Index));
        if (!IsOwner(track)) return Forbid();

        DeleteFile(track.AudioFilePath, "audio");
        DeleteFile(track.CoverImagePath, "covers");

        _context.Tracks.Remove(track);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Track deleted.";
        return RedirectToAction(nameof(Index));
    }

    // Helpers
    private bool IsOwner(Track track) => track.UploaderId == _userManager.GetUserId(User);

    private async Task ValidateFileAsync(IFormFile file, string field, string[] allowed, long maxBytes, string kind)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowed.Contains(ext))
        {
            ModelState.AddModelError(field, $"Allowed {kind} types: {string.Join(", ", allowed)}");
            return;
        }
        if (file.Length == 0)
        {
            ModelState.AddModelError(field, "The file is empty.");
            return;
        }
        if (file.Length > maxBytes)
        {
            ModelState.AddModelError(field, $"The {kind} file must be {maxBytes / (1024 * 1024)} MB or smaller.");
            return;
        }

        var header = new byte[12];
        int read;
        await using (var stream = file.OpenReadStream())
        {
            read = await stream.ReadAsync(header, 0, header.Length);
        }

        if (!HasValidSignature(ext, header, read))
            ModelState.AddModelError(field, $"This file does not look like a real {kind} file.");
    }

    private static bool HasValidSignature(string ext, byte[] h, int len)
    {
        if (len < 4) return false;

        bool Ascii(int offset, string text) =>
            len >= offset + text.Length &&
            text.Select((c, i) => h[offset + i] == (byte)c).All(x => x);

        return ext switch
        {
            ".mp3" => Ascii(0, "ID3") || (h[0] == 0xFF && (h[1] & 0xE0) == 0xE0),
            ".wav" => Ascii(0, "RIFF") && Ascii(8, "WAVE"),
            ".ogg" => Ascii(0, "OggS"),
            ".m4a" => Ascii(4, "ftyp"),
            ".jpg" or ".jpeg" => h[0] == 0xFF && h[1] == 0xD8 && h[2] == 0xFF,
            ".png" => h[0] == 0x89 && h[1] == 0x50 && h[2] == 0x4E && h[3] == 0x47,
            ".webp" => Ascii(0, "RIFF") && Ascii(8, "WEBP"),
            _ => false
        };
    }

    // Files are saved OUTSIDE wwwroot, so they can't be opened by URL directly.
    // They are only served by MediaController, which requires login.
    private async Task<string> SaveFileAsync(IFormFile file, string folder)
    {
        var folderPath = Path.Combine(_env.ContentRootPath, "PrivateUploads", folder);
        Directory.CreateDirectory(folderPath);

        var fileName = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName).ToLowerInvariant()}";
        var fullPath = Path.Combine(folderPath, fileName);

        using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return fileName;
    }

    private void DeleteFile(string? fileName, string folder)
    {
        if (string.IsNullOrEmpty(fileName)) return;

        var safeName = Path.GetFileName(fileName);
        var fullPath = Path.Combine(_env.ContentRootPath, "PrivateUploads", folder, safeName);

        if (System.IO.File.Exists(fullPath))
            System.IO.File.Delete(fullPath);
    }
}