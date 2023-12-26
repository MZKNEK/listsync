
namespace ListSync.Matcher;

public interface IPreMached
{
    public static readonly long kNotFound = -1;
    public static readonly long kIgnored = -2;

    public Task<List<Shinden.Models.IQuickSearch>> FilterShindenEntriesAsync(List<Shinden.Models.IQuickSearch> entries);
    public Task<long> GetShindenIdAsync(long anilistId);
    public Task IgnoreEntryAsync(long anilistId);
    public Task AddMatchedIdAsync(long anilistId, long shindenId, long malId = 0);
}