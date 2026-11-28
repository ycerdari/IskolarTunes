using IskolarTunes.Data;
using IskolarTunes.Models;
using IskolarTunes.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace IskolarTunes.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new HomeViewModel();

            // Guests only see the hero; track data stays behind login.
            if (User.Identity?.IsAuthenticated != true) return View(vm);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var since = DateTime.UtcNow.AddDays(-7);

            vm.NewTracks = await Project(
                    _context.Tracks.AsNoTracking().OrderByDescending(t => t.UploadedAt).Take(10),
                    userId, since)
                .ToListAsync();

            vm.Trending = await Project(_context.Tracks.AsNoTracking(), userId, since)
                .Where(x => x.WeeklyPlays > 0)
                .OrderByDescending(x => x.WeeklyPlays)
                .ThenByDescending(x => x.LikeCount)
                .Take(5)
                .ToListAsync();

            vm.Genres = await _context.Tracks.AsNoTracking()
                .Where(t => t.Genre != null && t.Genre != "")
                .GroupBy(t => t.Genre!)
                .Select(g => new HomeGenreViewModel { Name = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .ThenBy(g => g.Name)
                .Take(8)
                .ToListAsync();

            return View(vm);
        }

        private static IQueryable<HomeTrackViewModel> Project(IQueryable<Track> query, string? userId, DateTime since) =>
            query.Select(t => new HomeTrackViewModel
            {
                Id = t.Id,
                Title = t.Title,
                ArtistName = t.Uploader!.FullName,
                Genre = t.Genre,
                HasCover = t.CoverImagePath != null,
                WeeklyPlays = t.Plays.Count(p => p.PlayedAt >= since),
                LikeCount = t.Likes.Count(),
                LikedByMe = t.Likes.Any(l => l.UserId == userId)
            });

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Route("Home/Status/{code:int}")]
        public IActionResult Status(int code)
        {
            ViewData["Code"] = code;
            return View();
        }
    }
}