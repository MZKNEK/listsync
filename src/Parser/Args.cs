using AniListNet.Objects;
using SCLIAP;

namespace ListSync.Parser;

public class Args : ArgsHelper<Args>
{
    private SimpleCLIArgsParser<Args>? _parser;

    public bool Help;
    public int UserId;
    public string ApiKey;
    public Mode WorkMode;
    public MediaType Type;
    public string Marmolade;
    public bool TrustGuesses;
    public string ShindenCreds;
    public DirectoryInfo OutputPath;
    public FileInfo? CustomMatchesPath;

    public enum Mode { Walk, Update, Watch }

    public Args()
    {
        Help = true;
        ApiKey = "";
        UserId = -1;
        Marmolade = "";
        ShindenCreds = "";
        WorkMode = Mode.Walk;
        TrustGuesses = false;
        OutputPath = new("./");
        Type = MediaType.Anime;
        CustomMatchesPath = null;

        _parser = null;
    }

    public static Args Default => new();

    public void ParseArgs(string[] args)
    {
        _parser ??= Configure();
        var o = _parser.Parse(args);
        CustomMatchesPath = o.CustomMatchesPath;
        ShindenCreds = o.ShindenCreds;
        TrustGuesses = o.TrustGuesses;
        OutputPath = o.OutputPath;
        Marmolade = o.Marmolade;
        WorkMode = o.WorkMode;
        UserId = o.UserId;
        ApiKey = o.ApiKey;
        Type = o.Type;
        Help = o.Help;
    }

    public string GetHelp()
    {
        _parser ??= Configure();
        return _parser.GetHelp();
    }

    public override SimpleCLIArgsParser<Args> Configure(
        Configuration config = default!) =>
            new SimpleCLIArgsParser<Args>(config)
            .AddDefaultHelpOptions(True(Help))
            .AddOption(new(True(TrustGuesses),
            "trust automated guesses",
            name: 't'))
            .AddOption(new((arg, nextArg) =>
            {
                if (int.TryParse(nextArg, out arg.UserId))
                {
                    arg.Help = false;
                }
            },
            "anilist user id",
            name: 'u',
            needNextArgument: true))
            .AddOption(new((arg, nextArg) =>
            {
                arg.WorkMode = nextArg.ToLower() switch
                {
                    "walk" => Mode.Walk,
                    "update" => Mode.Update,
                    "watch" => Mode.Watch,
                    _ => throw new Exception("Unsuported work mode!")
                };
            },
            "program work mode (walk/update/watch)",
            name: 'w',
            needNextArgument: true))
            .AddOption(new((arg, _) =>
            {
                arg.Type = MediaType.Manga;
            },
            "parse manga list insted of anime",
            name: 'm'))
            .AddOption(new((arg, nextArg) =>
                {
                    if (!Path.Exists(nextArg))
                    {
                        throw new Exception($"Path {nextArg} don't exist!");
                    }
                    arg.OutputPath = new(nextArg);
                },
            "output location",
            name: 'o',
            needNextArgument: true))
            .AddOption(new((arg, nextArg) =>
                {
                    if (!Path.Exists(nextArg))
                    {
                        throw new Exception($"Path {nextArg} don't exist!");
                    }
                    arg.CustomMatchesPath = new(nextArg);
                },
            "custom match file(walker)/list(updater)",
            name: 'f',
            needNextArgument: true))
            .AddOption(new((arg, nextArg) =>
                {
                    arg.ApiKey = nextArg;
                },
            "shinden apikey",
            name: 'k',
            needNextArgument: true))
            .AddOption(new((arg, nextArg) =>
                {
                    arg.ShindenCreds = nextArg;
                },
            "shinden credentials (username@password)",
            name: 'l',
            needNextArgument: true))
            .AddOption(new((arg, nextArg) =>
                {
                    arg.Marmolade = nextArg;
                },
            "marmolade?",
            name: 'p',
            needNextArgument: true));
}