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

        public async Task<IActionResult> Index(string? college, string? genre)
        {
            var vm = new HomeViewModel();

            // Guests only see the hero; track data stays behind login.
            if (User.Identity?.IsAuthenticated != true) return View(vm);

            if (college != null && !Colleges.Filters.Contains(college)) college = null;
            genre = string.IsNullOrWhiteSpace(genre) ? null : genre.Trim();
            if (genre != null && genre.Length > 100) genre = null;
            vm.College = college;
            vm.Genre = genre;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var since = DateTime.UtcNow.AddDays(-7);
            var all = _context.Tracks.AsNoTracking();

            vm.Latest = await Project(all.OrderByDescending(t => t.UploadedAt).Take(10), userId, since)
                .ToListAsync();

            if (vm.Filtered)
            {
                var shelf = all;
                if (college != null) shelf = shelf.Where(t => t.Uploader!.College == college);
                if (genre != null) shelf = shelf.Where(t => t.Genre == genre);

                vm.Shelf = await Project(shelf.OrderByDescending(t => t.UploadedAt).Take(10), userId, since)
                    .ToListAsync();
            }
            else
            {
                vm.Shelf = vm.Latest;
            }

            vm.Trending = await Project(all, userId, since)
                .Where(x => x.WeeklyPlays > 0)
                .OrderByDescending(x => x.WeeklyPlays)
                .ThenByDescending(x => x.LikeCount)
                .Take(5)
                .ToListAsync();

            vm.Genres = await all
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
                ArtistId = t.UploaderId,
                ArtistName = t.Uploader!.FullName,
                College = t.Uploader!.College,
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