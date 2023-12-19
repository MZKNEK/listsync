using System.Text;
using AniListNet;
using AniListNet.Objects;
using Shinden;

namespace ListSync.Modules;

class WalkMatchAndGenerate
{
    private readonly Parser.Args _args;
    private readonly ListWalker.AniWalker _walker;
    private readonly Matcher.ShindenTitleMatcher _finder;
    private readonly Matcher.ShindenPreMatchedWithAni _preMatched;

    public WalkMatchAndGenerate(Parser.Args arg)
    {
        _args = arg;

        _walker = new ListWalker.AniWalker(new AniClient(), _args.Type, _args.UserId);
        _finder = new Matcher.ShindenTitleMatcher(new ShindenClient(new Auth(_args.ApiKey, "ListSync")));
        _preMatched = new Matcher.ShindenPreMatchedWithAni(_args.CustomMatchesPath);
    }

    public async Task RunAsync()
    {
        var output = new StringBuilder();
        long matched = 0;

        while(_walker.CanWalk())
        {
            var delay = Task.Delay(TimeSpan.FromSeconds(10));
            var lists = await _walker.GetNextAsync();

            foreach(var list in lists)
            {
                foreach(var title in list.Entries)
                {
                    var shindenId = _preMatched.GetShindenId(title.Media.Id);
                    if (shindenId == Matcher.ShindenPreMatchedWithAni.kIgnored)
                        continue;

                    if (shindenId != Matcher.ShindenPreMatchedWithAni.kNotFound)
                    {
                        matched++;
                        AddEntryToOutput(output, shindenId, title);
                        continue;
                    }

                    var shindenTitles = await _finder.FindMatchAsync(title);
                    shindenTitles = _preMatched.FilterShindenEntries(shindenTitles);
                    if (shindenTitles.Count == 1 && TitleMatchesExacly(shindenTitles[0].Title, title))
                    {
                        matched++;
                        shindenId = (long) shindenTitles[0].Id;
                        AddEntryToOutput(output, shindenId, title);
                        _preMatched.AddMatchedId(title.Media.Id, shindenId);
                        continue;
                    }

                    Console.Clear();
                    Console.WriteLine($"Matched: {matched}");
                    Console.WriteLine($"Matching: {title.Media.Title.PreferredTitle}");
                    Console.WriteLine($"Url: {title.Media.Url}\n");

                    if (shindenTitles.Count == 0)
                    {
                        Console.WriteLine("Match not found, press enter to continue...");
                        Console.ReadLine();
                        continue;
                    }

                    var prob = shindenTitles.FirstOrDefault(x => TitleMatchesExacly(x.Title, title));
                    if (prob is not null && _args.TrustGuesses)
                    {
                        if (await _finder.VerifyGuessAsync(prob, title))
                        {
                            matched++;
                            shindenId = (long)prob.Id;
                            AddEntryToOutput(output, shindenId, title);
                            _preMatched.AddMatchedId(title.Media.Id, shindenId);
                            continue;
                        }
                    }

                    for (int i = 0; i < shindenTitles.Count; i++)
                    {
                        if (prob is not null && prob.Id == shindenTitles[i].Id)
                            Console.Write("* ");

                        Console.WriteLine($"[{i + 1}] {shindenTitles[i].Title}");
                        Console.WriteLine($"Url: https://shinden.pl/t/{shindenTitles[i].Id}\n");
                    }

                    Console.WriteLine($"[{shindenTitles.Count + 1}] Skip...\n");
                    Console.WriteLine($"[{shindenTitles.Count + 2}] Manual...\n");
                    Console.WriteLine($"[{shindenTitles.Count + 3}] Ignore...\n");
                    Console.WriteLine("Select option:");

                    int selected = -1;
                    while (selected < 0 || selected > shindenTitles.Count + 3)
                    {
                        var userLine = Console.ReadLine();
                        int.TryParse(userLine, out selected);
                    }

                    if (selected == shindenTitles.Count + 1)
                        continue;

                    if (selected == shindenTitles.Count + 2)
                    {
                        Console.WriteLine("Shinden id:");
                        bool ok = false;
                        while (!ok)
                        {
                            var userLine = Console.ReadLine();
                            ok = long.TryParse(userLine, out shindenId);
                        }

                        matched++;
                        AddEntryToOutput(output, shindenId, title);
                        _preMatched.AddMatchedId(title.Media.Id, shindenId);
                        continue;
                    }

                    if (selected == shindenTitles.Count + 3)
                    {
                        _preMatched.IgnoreEntry(title.Media.Id);
                        continue;
                    }

                    matched++;
                    shindenId = (long) shindenTitles[selected - 1].Id;
                    AddEntryToOutput(output, shindenId, title);
                    _preMatched.AddMatchedId(title.Media.Id, shindenId);
                }
            }

            Console.WriteLine("Waiting...");
            await delay;
        }

        if (!_args.OutputPath.Exists) _args.OutputPath.Create();
        File.WriteAllText($"{_args.OutputPath.FullName}\\{_args.Type}mlist", output.ToString());
    }

    internal static bool TitleMatchesExacly(string shindenTitle, MediaEntry title)
    {
        shindenTitle = shindenTitle.Trim();
        if (title.Media.Title.RomajiTitle.Equals(shindenTitle, StringComparison.InvariantCultureIgnoreCase))
            return true;

        if (title.Media.Title.NativeTitle.Equals(shindenTitle, StringComparison.InvariantCultureIgnoreCase))
            return true;

        if (title.Media.Title.EnglishTitle is not null && title.Media.Title.EnglishTitle.Equals(shindenTitle, StringComparison.InvariantCultureIgnoreCase))
            return true;

        return false;
    }

    internal static void AddEntryToOutput(StringBuilder o, long shindenId, MediaEntry title)
        => o.AppendLine($"{shindenId}:{title.Score}:{title.Progress}:{title.Status}");
}