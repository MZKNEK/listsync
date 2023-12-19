using AniListNet;
using AniListNet.Objects;

namespace ListSync.ListWalker;

class AniWalker
{
    private readonly int _userId;
    private readonly int _perPage;
    private readonly AniClient _client;
    private readonly MediaType _listType;

    private int currentPage;
    private bool canWalk;

    public AniWalker(AniClient client, MediaType type, int userId, int pageLimit = 500)
    {
        _perPage = pageLimit;
        _listType = type;
        _userId = userId;
        _client = client;

        currentPage = 1;
        canWalk = true;
    }

    public async Task<MediaEntryList[]> GetNextAsync()
    {
        if (!canWalk)
            return [];

        try
        {
            var res = await _client.GetUserEntryCollectionAsync(_userId, _listType, new AniPaginationOptions(currentPage, _perPage)).ConfigureAwait(false);
            canWalk = res.HasNextChunk;
            currentPage++;
            return res.Lists;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            await Task.Delay(TimeSpan.FromSeconds(60));
            return [];
        }
    }

    public bool CanWalk() => canWalk;
}