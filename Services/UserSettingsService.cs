using IskolarTunes.Data;
using Microsoft.EntityFrameworkCore;

namespace IskolarTunes.Services;

public class UserSettingsService
{
    private readonly ApplicationDbContext _db;

    public UserSettingsService(ApplicationDbContext db) => _db = db;

    // Every catalog key, using the saved value when valid, otherwise the default.
    public async Task<Dictionary<string, string>> GetAllAsync(string userId)
    {
        var saved = await _db.UserSettings.AsNoTracking()
            .Where(s => s.UserId == userId)
            .ToDictionaryAsync(s => s.Key, s => s.Value);

        return SettingsCatalog.All.ToDictionary(
            d => d.Key,
            d => saved.TryGetValue(d.Key, out var v) && SettingsCatalog.IsValid(d, v) ? v : d.Default);
    }

    // Returns false for an unknown key, a Coming soon setting, or an out-of-range value.
    public async Task<bool> SetAsync(string userId, string key, string value)
    {
        var def = SettingsCatalog.Find(key);
        if (def == null || def.Status != SettingStatus.Live || !SettingsCatalog.IsValid(def, value))
            return false;

        var row = await _db.UserSettings.FirstOrDefaultAsync(s => s.UserId == userId && s.Key == key);
        if (row == null)
            _db.UserSettings.Add(new UserSetting { UserId = userId, Key = key, Value = value });
        else
            row.Value = value;

        await _db.SaveChangesAsync();
        return true;
    }

    public Task ResetAsync(string userId) =>
        _db.UserSettings.Where(s => s.UserId == userId).ExecuteDeleteAsync();
}