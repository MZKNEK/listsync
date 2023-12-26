using Microsoft.EntityFrameworkCore;

namespace ListSync.Modules;

class DbFetcher
{
    private readonly string _conn;

    public DbFetcher(string connection)
    {
        _conn = connection;
    }

    public async Task UploadFromFileAsync(FileInfo? filePath)
    {
        if (filePath is null || filePath.Directory is null)
            throw new Exception("No file!");

        if (!filePath.Exists) filePath.Create();

        using var db = new Database.Dbc(_conn);
        foreach (var line in File.ReadAllLines(filePath!.FullName))
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

    public async Task AddNewRelationAsync(Database.AniShinRelation rel)
    {
        using var db = new Database.Dbc(_conn);
        await db.AnimeRelation.AddAsync(rel);
        await db.SaveChangesAsync();
    }

    internal static Database.AniShinRelation ToAniShin(long aniid, long shinid)
        => new() { AnilistId = aniid, ShindenId = shinid, IsVerified = false };
}