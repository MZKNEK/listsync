using AniListNet.Objects;
using Shinden;
using Shinden.Models;

namespace ListSync.Matcher;

class ShindenTitleMatcher
{
    public enum AutoMatchBy
    {
        Romaji, English, Native, UserPreferred, All
    }

    private readonly ShindenClient _client;
    private readonly AutoMatchBy _matchBy;

    public ShindenTitleMatcher(ShindenClient client, AutoMatchBy match = AutoMatchBy.All)
    {
        _client = client;
        _matchBy = match;
    }

    public async Task<List<IQuickSearch>> FindMatchAsync(string title, QuickSearchType type)
    {
        var res = await _client.Search.QuickSearchAsync(title, type);
        if (res.IsSuccessStatusCode())
            return res.Body;

        return [];
    }

    public async Task<List<IQuickSearch>> FindMatchAsync(MediaEntry entry)
    {
        var type = entry.Media.Type switch
        {
            MediaType.Anime => QuickSearchType.Anime,
            MediaType.Manga => QuickSearchType.Manga,
            _ => throw new Exception("Unsuported media type!")
        };

        if (_matchBy == AutoMatchBy.All)
            return await FindMatchAllAsync(entry, type).ConfigureAwait(false);

        var title = _matchBy switch
        {
            AutoMatchBy.English         => entry.Media.Title.EnglishTitle,
            AutoMatchBy.Native          => entry.Media.Title.NativeTitle,
            AutoMatchBy.Romaji          => entry.Media.Title.RomajiTitle,
            AutoMatchBy.UserPreferred   => entry.Media.Title.PreferredTitle,
            _ => throw new Exception("Unsuported matching type!")
        };

        var res = await _client.Search.QuickSearchAsync(title, type).ConfigureAwait(false);
        if (res.IsSuccessStatusCode())
            return res.Body;

        return [];
    }

    public async Task<bool> VerifyGuessAsync(IIndexable id, MediaEntry title)
    {
        var res = await _client.Title.GetInfoAsync(id).ConfigureAwait(false);
        if (res.IsSuccessStatusCode())
        {
            var startDate = res.Body.StartDate.Date;
            var aniStartDate = title.Media.StartDate;
            return startDate.Day == aniStartDate.Day && startDate.Month == aniStartDate.Month && startDate.Year == aniStartDate.Year;
        }
        return false;
    }

    internal async Task<List<IQuickSearch>> FindMatchAllAsync(MediaEntry entry, QuickSearchType type)
    {
        var list = new List<IQuickSearch>();

        var res = await _client.Search.QuickSearchAsync(entry.Media.Title.RomajiTitle, type).ConfigureAwait(false);
        if (res.IsSuccessStatusCode())
            list.AddRange(res.Body);

        res = await _client.Search.QuickSearchAsync(entry.Media.Title.NativeTitle, type).ConfigureAwait(false);
        if (res.IsSuccessStatusCode())
            list.AddRange(res.Body);

        if (entry.Media.Title.EnglishTitle is not null)
        {
            res = await _client.Search.QuickSearchAsync(entry.Media.Title.EnglishTitle, type).ConfigureAwait(false);
            if (res.IsSuccessStatusCode())
                list.AddRange(res.Body);
        }

        if (list.Count == 0)
        {
            var spacedTitle = entry.Media.Title.PreferredTitle.Split(" ");
            while (list.Count == 0 && spacedTitle.Length > 1)
            {
                spacedTitle = spacedTitle[..^1];
                res = await _client.Search.QuickSearchAsync(string.Join(" ", spacedTitle), type).ConfigureAwait(false);
                if (res.IsSuccessStatusCode())
                    list.AddRange(res.Body);
            }
        }

        return list.DistinctBy(x => x.Id).ToList();
    }
}