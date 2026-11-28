using IskolarTunes.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IskolarTunes.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Track> Tracks => Set<Track>();
    public DbSet<Playlist> Playlists => Set<Playlist>();
    public DbSet<PlaylistTrack> PlaylistTracks => Set<PlaylistTrack>();
    public DbSet<TrackLike> TrackLikes { get; set; }
    public DbSet<TrackPlay> TrackPlays { get; set; }
    public DbSet<UserSetting> UserSettings => Set<UserSetting>();
    public DbSet<UserSocialLink> UserSocialLinks => Set<UserSocialLink>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>()
            .Property(u => u.PublicFields)
            .HasDefaultValue(ProfileField.FullName | ProfileField.College);

        builder.Entity<UserSetting>(e =>
        {
            e.HasIndex(s => new { s.UserId, s.Key }).IsUnique();
            e.HasOne(s => s.User).WithMany().HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<UserSocialLink>(e =>
        {
            e.Property(l => l.Platform).HasConversion<string>().HasMaxLength(20);
            e.HasOne(l => l.User).WithMany(u => u.SocialLinks).HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // One like per user per track
        builder.Entity<TrackLike>()
            .HasIndex(l => new { l.UserId, l.TrackId })
            .IsUnique();

        builder.Entity<TrackLike>()
            .HasOne(l => l.Track)
            .WithMany(t => t.Likes)
            .HasForeignKey(l => l.TrackId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<TrackLike>()
            .HasOne(l => l.User)
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        // Index for fast "plays in the last 7 days" queries
        builder.Entity<TrackPlay>()
            .HasIndex(p => new { p.TrackId, p.PlayedAt });

        builder.Entity<TrackPlay>()
            .HasOne(p => p.Track)
            .WithMany(t => t.Plays)
            .HasForeignKey(p => p.TrackId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<TrackPlay>()
            .HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<PlaylistTrack>()
            .HasKey(pt => new { pt.PlaylistId, pt.TrackId });

        builder.Entity<PlaylistTrack>()
            .HasOne(pt => pt.Playlist)
            .WithMany(p => p.PlaylistTracks)
            .HasForeignKey(pt => pt.PlaylistId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<PlaylistTrack>()
            .HasOne(pt => pt.Track)
            .WithMany(t => t.PlaylistTracks)
            .HasForeignKey(pt => pt.TrackId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict avoids SQL Server "multiple cascade paths" errors
        builder.Entity<Track>()
            .HasOne(t => t.Uploader)
            .WithMany()
            .HasForeignKey(t => t.UploaderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Playlist>()
            .HasOne(p => p.Owner)
            .WithMany()
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}