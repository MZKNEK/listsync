using System.Text;
using ListSync.Modules;

namespace ListSync;

public class ListSync
{
    private static string _version = "1.0.0";
    private static StringBuilder _header = new StringBuilder()
        .AppendLine($"ListSync # {_version}")
        .AppendLine("-----------------------------");

    static void Main(string[] args) => new ListSync().MainAsync(args).GetAwaiter().GetResult();

    public async Task MainAsync(string[] args)
    {
        var arg = new Parser.Args();
        try
        {
            arg.ParseArgs(args);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{_header}{ex.Message}");
            return;
        }

        if (arg.Help)
        {
            Console.WriteLine($"{_header}{arg.GetHelp()}");
            return;
        }

        if (arg.WorkMode == Parser.Args.Mode.Walk)
        {
            var walker = new WalkMatchAndGenerate(arg);
            await walker.RunAsync();
            return;
        }

        if (arg.WorkMode == Parser.Args.Mode.Update)
        {
            var updater = new UpdateShindenList(arg);
            await updater.RunAsync();
            return;
        }
    }
}
