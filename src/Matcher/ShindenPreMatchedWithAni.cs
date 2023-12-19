namespace ListSync.Matcher;

class ShindenPreMatchedWithAni
{
    private readonly Dictionary<long, long> _dic;
    private readonly FileInfo? _file;

    public static int kNotFound = -1;
    public static int kIgnored = -2;

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

    public List<Shinden.Models.IQuickSearch> FilterShindenEntries(List<Shinden.Models.IQuickSearch> entries)
    {
        return entries.Where(x => _dic.All(c => c.Value != (long) x.Id)).ToList();
    }

    public long GetShindenId(long anilistId)
    {
        if (_dic.TryGetValue(anilistId, out var shindenId))
        {
            return shindenId;
        }
        return kNotFound;
    }

    public void IgnoreEntry(long anilistId) => AddMatchedId(anilistId, kIgnored);

    public void AddMatchedId(long anilistId, long shindenId)
    {
        _dic.Add(anilistId, shindenId);
        if (_file is not null && _file.Exists)
        {
            using (var stream = _file.AppendText())
            {
                stream.WriteLine($"{anilistId}:{shindenId}");
            }
        }
    }
}