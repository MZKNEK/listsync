using Microsoft.EntityFrameworkCore;

namespace ListSync.Modules;

class DbFetcher
{
    private readonly FileInfo? _file;
    private readonly string _conn;

    public DbFetcher(FileInfo? filePath, string connection)
    {
        _conn = connection;
        _file = filePath;

        if (_file is null || _file.Directory is null)
            throw new Exception("No file!");

        if (!_file.Exists) _file.Create();
    }

    public async Task UploadAsync()
    {
        using var db = new Database.Dbc(_conn);
        foreach (var line in File.ReadAllLines(_file!.FullName))
        {
            var elements = line.Split(':');
            if (elements.Length == 2 && long.TryParse(elements[0], out var aniid)
                && long.TryParse(elements[1], out var shinid))
            {
                await db.AnimeRelation.AddAsync(ToAniShin(aniid, shinid));
            }
        }
        await db.SaveChangesAsync();
    }

    public async Task<List<Database.AniShinRelation>> DownloadAsync()
    {
        using var db = new Database.Dbc(_conn);
        return await db.AnimeRelation.ToListAsync();
    }

    internal static Database.AniShinRelation ToAniShin(long aniid, long shinid)
        => new() { AnilistId = aniid, ShindenId = shinid, IsVerified = false };
}