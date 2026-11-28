namespace IskolarTunes.Services;

public enum SettingKind { Toggle, Choice, Slider }
public enum SettingStatus { Live, ComingSoon }

// Toggle values are "0"/"1". Choice values are the option index. Slider values are the number.
public record SettingDef(
    string Key, int Section, SettingKind Kind, string Label, string Description,
    string[] Options, int Min, int Max, string Unit, string Default,
    SettingStatus Status = SettingStatus.Live);

public static class SettingsCatalog
{
    public static readonly string[] Sections =
    {
        "Account", "Security", "Playback", "Audio quality", "Display", "Notifications",
        "Privacy", "Library & storage", "Accessibility", "Shortcuts", "Artist tools",
        "Advanced", "About"
    };

    private const SettingStatus Soon = SettingStatus.ComingSoon;

    private static SettingDef T(string key, int sec, string label, string desc, bool def,
        SettingStatus st = SettingStatus.Live) =>
        new(key, sec, SettingKind.Toggle, label, desc, Array.Empty<string>(), 0, 1, "", def ? "1" : "0", st);

    private static SettingDef C(string key, int sec, string label, string desc, string[] opts, int def,
        SettingStatus st = SettingStatus.Live) =>
        new(key, sec, SettingKind.Choice, label, desc, opts, 0, opts.Length - 1, "", def.ToString(), st);

    private static SettingDef S(string key, int sec, string label, string desc, int min, int max,
        string unit, int def, SettingStatus st = SettingStatus.Live) =>
        new(key, sec, SettingKind.Slider, label, desc, Array.Empty<string>(), min, max, unit, def.ToString(), st);

    public static readonly IReadOnlyList<SettingDef> All = new List<SettingDef>
    {
        // 0 Account
        C("lang", 0, "Language", "Interface language", new[] { "English", "Filipino" }, 0, Soon),

        // 1 Security
        T("alertlogin", 1, "Login alerts", "Email me when a new device signs in", true, Soon),

        // 2 Playback
        T("autoplay", 2, "Autoplay", "Keep playing similar songs when your queue ends", true),
        S("xfade", 2, "Crossfade", "Blend the end of a song into the next", 0, 12, " s", 0, Soon),
        T("gapless", 2, "Gapless playback", "Remove silence between tracks", true, Soon),
        T("norm", 2, "Normalize volume", "Keep loudness the same across songs", true, Soon),
        T("fair", 2, "Fair Rotation discovery", "Give every artist an equal turn instead of ranking by popularity", true, Soon),
        C("rep", 2, "Default repeat", "What happens at the end of a playlist", new[] { "Off", "All", "One" }, 0),

        // 3 Audio quality
        C("eq", 3, "Equalizer", "Preset sound profile", new[] { "Flat", "Bass boost", "Vocal", "Acoustic" }, 0, Soon),
        T("mono", 3, "Mono audio", "Play the same sound in both ears", false, Soon),

        // 4 Display
        C("theme", 4, "Theme", "Choose light or dark", new[] { "Dark", "Light" }, 0),
        T("spin", 4, "Spinning vinyl", "Animate the record while music plays", true),
        C("dens", 4, "Density", "How tightly lists are packed", new[] { "Comfortable", "Compact" }, 0),
        S("txt", 4, "Text size", "Make text larger or smaller", 85, 130, "%", 100),
        T("showpnl", 4, "Open Now playing panel automatically", "Show the panel when a song starts", false, Soon),

        // 5 Notifications (saved now; dedications and email features read them later)
        T("nrel", 5, "New releases", "From artists you follow", true),
        T("nded", 5, "Dedications for me", "When someone dedicates a song to you", true),
        T("ngig", 5, "Gig reminders", "Before events from artists you follow", true),
        T("nprod", 5, "Product updates", "News about new IskolarTunes features", false),
        C("digest", 5, "Email digest", "How often we email you", new[] { "Never", "Weekly", "Monthly" }, 1),

        // 6 Privacy
        T("priv", 6, "Private session", "Listen without it showing in your activity", false),
        T("actv", 6, "Share listening activity", "Let friends see what you play", true, Soon),
        C("vis", 6, "Profile visibility", "Who can see your profile", new[] { "Everyone", "Only me" }, 0),
        T("favpub", 6, "Show favorites on profile", "Let others see your liked songs", false),
        T("anon", 6, "Dedicate anonymously by default", "Hide your name on new dedications", false),
        T("recv", 6, "Allow dedications to me", "Let others dedicate songs to you", true),
        T("expl", 6, "Allow explicit content", "Show songs marked explicit", true, Soon),

        // 7 Library & storage
        C("libv", 7, "Library view", "How your library is laid out", new[] { "Grid", "List" }, 0, Soon),
        C("sort", 7, "Sort library by", "Default order", new[] { "Recents", "Alphabetical", "Creator" }, 0, Soon),
        T("favfirst", 7, "Pin Liked songs first", "Keep favorites at the top of your library", true, Soon),

        // 8 Accessibility
        T("rmot", 8, "Reduce motion", "Turn off animations", false),
        T("hic", 8, "High contrast", "Stronger colors and borders", false),
        T("announce", 8, "Announce track changes", "Tell screen readers when a new song starts", true, Soon),
        T("focus", 8, "Always show focus outlines", "Make keyboard focus easy to see", true),

        // 9 Shortcuts
        T("keyson", 9, "Keyboard shortcuts", "Turn the shortcuts below on or off", true, Soon),

        // 10 Artist tools
        C("dgen", 10, "Default genre", "Pre-filled on new uploads", new[] { "OPM", "Pop", "Rock", "Hip-Hop" }, 0),
        T("pubplays", 10, "Show play counts", "Let listeners see how often a song is played", true),
        T("allowded", 10, "Allow dedications on my songs", "Let people dedicate your songs", true),
    };

    private static readonly Dictionary<string, SettingDef> ByKey = All.ToDictionary(d => d.Key);

    public static SettingDef? Find(string key) => ByKey.GetValueOrDefault(key);

    public static bool IsValid(SettingDef def, string value) =>
        int.TryParse(value, out var n) && n >= def.Min && n <= def.Max;
}