using Shinden;
using Shinden.Models;

namespace ListSync.Modules;

class UpdateShindenList
{
    private readonly Parser.Args _args;
    private readonly ShindenClient _client;

    public UpdateShindenList(Parser.Args arg)
    {
        _args = arg;
        _client = new ShindenClient(new Auth(_args.ApiKey, "ListSync", _args.Marmolade));
    }

    public async Task RunAsync()
    {
        var creds = _args.ShindenCreds.Split("@");
        await _client.User.LoginAsync(new UserAuth(creds[0], creds[1]));

        if (_args.CustomMatchesPath is not null && _args.CustomMatchesPath.Directory is not null)
        {
            if (!_args.CustomMatchesPath.Exists) throw new Exception("File not found!");
            var allLines = File.ReadAllLines(_args.CustomMatchesPath.FullName);
            long index = 0; long total = allLines.Count();

            foreach (var line in allLines)
            {
                var elements = line.Split(':');
                if (elements.Length == 4 && ulong.TryParse(elements[0], out var titleId)
                    && uint.TryParse(elements[1], out var score)
                    && long.TryParse(elements[2], out var episodes)
                    && TryParseType(elements[3], out var type))
                {
                    await _client.User.OnLoggedIn.ChangeTitleStatusAsync(type, titleId);
                    if (_args.Type == AniListNet.Objects.MediaType.Anime)
                    {
                        await _client.User.OnLoggedIn.RateAnimeAsync(titleId, AnimeRateType.Total, score);
                    }
                    else
                    {
                        await _client.User.OnLoggedIn.RateMangaAsync(titleId, MangaRateType.Total, score);
                    }
                }

                if (++index % 15 == 0)
                {
                    Console.Clear();
                    Console.WriteLine($"{index}/{total}");
                }
            }
        }
    }

    internal static bool TryParseType(string s, out ListType type)
    {
        type = s.ToLower() switch
        {
            "completed" => ListType.Completed,
            "dropped" => ListType.Dropped,
            "paused" => ListType.Hold,
            "planning" => ListType.Plan,
            "current" => ListType.InProgress,
            _ => ListType.Skip
        };
        return type != ListType.Skip;
    }
}