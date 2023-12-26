using ListSync.Parser;

namespace ListSync.Matcher;

class ShindenDbRelation : IPreMached
{
    private readonly List<Database.AniShinRelation> _rel;
    private readonly Modules.DbFetcher _fetcher;

    public ShindenDbRelation(Args args)
    {
        _fetcher = new Modules.DbFetcher(args.Connection);
        _rel = _fetcher.DownloadAsync().GetAwaiter().GetResult();
    }

    public Task<List<Shinden.Models.IQuickSearch>> FilterShindenEntriesAsync(List<Shinden.Models.IQuickSearch> entries)
    {
        return Task.FromResult(entries.Where(x => _rel.All(c => c.ShindenId != (long) x.Id)).ToList());
    }

    public Task<long> GetShindenIdAsync(long anilistId)
    {
        var thisRel = _rel.Where(x => x.AnilistId == anilistId).FirstOrDefault();
        if (thisRel is not null)
        {
            return Task.FromResult(thisRel.ShindenId);
        }
        return Task.FromResult(IPreMached.kNotFound);
    }

    public Task IgnoreEntryAsync(long anilistId) => AddMatchedIdAsync(anilistId, IPreMached.kIgnored);

    public async Task AddMatchedIdAsync(long anilistId, long shindenId, long malId = 0)
    {
        var nr = new Database.AniShinRelation
        {
            AnilistId = anilistId,
            ShindenId = shindenId,
            MyAnimeListId = malId,
            IsProposedChange = false,
            IsVerified = false,
        };

        _rel.Add(nr);
        await _fetcher.AddNewRelationAsync(nr);
        Console.WriteLine($"Saved to db: A {anilistId} - S {shindenId}");
    }
}