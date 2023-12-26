namespace ListSync.Matcher;

class ShindenPreMatchedWithAni : IPreMached
{
    private readonly Dictionary<long, long> _dic;
    private readonly FileInfo? _file;

    public ShindenPreMatchedWithAni(FileInfo? filePath)
    {
        _dic = [];
        _file = filePath;

        if (_file is not null && _file.Directory is not null)
        {
            if (!_file.Exists) _file.Create();
            foreach (var line in File.ReadAllLines(_file.FullName))
            {
                var elements = line.Split(':');
                if (elements.Length == 2 && long.TryParse(elements[0], out var aniid)
                    && long.TryParse(elements[1], out var shinid))
                {
                    _dic.Add(aniid, shinid);
                }
            }
        }
    }

    public Task<List<Shinden.Models.IQuickSearch>> FilterShindenEntriesAsync(List<Shinden.Models.IQuickSearch> entries)
    {
        return Task.FromResult(entries.Where(x => _dic.All(c => c.Value != (long) x.Id)).ToList());
    }

    public Task<long> GetShindenIdAsync(long anilistId)
    {
        if (_dic.TryGetValue(anilistId, out var shindenId))
        {
            return Task.FromResult(shindenId);
        }
        return Task.FromResult(IPreMached.kNotFound);
    }

    public Task IgnoreEntryAsync(long anilistId) => AddMatchedIdAsync(anilistId, IPreMached.kIgnored);

    public Task AddMatchedIdAsync(long anilistId, long shindenId, long malId = 0)
    {
        _dic.Add(anilistId, shindenId);
        if (_file is not null && _file.Exists)
        {
            using (var stream = _file.AppendText())
            {
                stream.WriteLine($"{anilistId}:{shindenId}");
            }
        }
        return Task.CompletedTask;
    }
}