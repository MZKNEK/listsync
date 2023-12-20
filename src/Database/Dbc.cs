using Microsoft.EntityFrameworkCore;

namespace ListSync.Database;

class Dbc : DbContext
{
    private readonly string _connection;

    public Dbc(string connection) : base()
    {
        _connection = connection;
    }

    public DbSet<AniShinRelation> AnimeRelation { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySql(_connection, new MySqlServerVersion(new Version(8, 0)));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AniShinRelation>(e =>
        {
            e.HasKey(v => v.Id);
            e.HasIndex(v => v.AnilistId).IsUnique();
            e.HasIndex(v => v.ShindenId);
            e.HasIndex(v => v.IsVerified);
            e.HasIndex(v => v.ChangeToShindenId);
            e.HasIndex(v => v.ChangeToAnilistId);
        });
    }
}
