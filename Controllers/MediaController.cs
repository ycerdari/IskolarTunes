using IskolarTunes.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IskolarTunes.Controllers;

[Authorize]
public class MediaController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    public MediaController(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [HttpGet("media/audio/{id:int}")]
    public async Task<IActionResult> Audio(int id)
    {
        var fileName = await _context.Tracks.AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => t.AudioFilePath)
            .FirstOrDefaultAsync();

        return ServeFile(fileName, "audio");
    }

    [HttpGet("media/cover/{id:int}")]
    public async Task<IActionResult> Cover(int id)
    {
        var fileName = await _context.Tracks.AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => t.CoverImagePath)
            .FirstOrDefaultAsync();

        return ServeFile(fileName, "covers");
    }

    private IActionResult ServeFile(string? fileName, string folder)
    {
        if (string.IsNullOrEmpty(fileName)) return NotFound();

        var safeName = Path.GetFileName(fileName);
        var fullPath = Path.Combine(_env.ContentRootPath, "PrivateUploads", folder, safeName);
        if (!System.IO.File.Exists(fullPath)) return NotFound();

        var contentType = Path.GetExtension(safeName).ToLowerInvariant() switch
        {
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            ".ogg" => "audio/ogg",
            ".m4a" => "audio/mp4",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };

        Response.Headers.CacheControl = "private, max-age=600";
        return PhysicalFile(fullPath, contentType, enableRangeProcessing: true);
    }
}