namespace ListSync.Database;

public class AniShinRelation
{
    public ulong Id { get; set; }
    public long ShindenId { get; set; }
    public long AnilistId { get; set; }
    public long MyAnimeListId { get; set; }
    public bool IsVerified { get; set; }
    public bool IsProposedChange { get; set; }
    public long ChangeToShindenId { get; set; }
    public long ChangeToAnilistId { get; set; }
}