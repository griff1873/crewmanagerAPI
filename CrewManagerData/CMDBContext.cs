using Microsoft.EntityFrameworkCore;
using CrewManagerData.Models;
using Microsoft.Extensions.Configuration;

namespace CrewManagerData;
// CMDBContext is the main database context for the Crew Manager application.
// It inherits from DbContext and is used to interact with the database.
public class CMDBContext : DbContext
{
    public CMDBContext(DbContextOptions<CMDBContext> options) : base(options)
    {
    }

    public DbSet<Profile> Profiles { get; set; } = null!;
    public DbSet<Auth0User> Auth0Users { get; set; } = null!;
    public DbSet<Auth0Identity> Auth0Identities { get; set; } = null!;
    public DbSet<Auth0ProfileData> Auth0ProfileData { get; set; } = null!;

    public DbSet<Event> Events { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();
        optionsBuilder.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure existing models
        modelBuilder.Entity<Profile>().ToTable("Profiles");

        // Configure Auth0 models
        modelBuilder.Entity<Auth0User>(entity =>
        {
            entity.ToTable("auth0_users");
            entity.HasIndex(e => e.Auth0UserId).IsUnique();
            entity.HasIndex(e => e.Email);
        });

        modelBuilder.Entity<Auth0Identity>(entity =>
        {
            entity.ToTable("auth0_identities");
            entity.HasIndex(e => new { e.Provider, e.UserId }).IsUnique();
        });

        modelBuilder.Entity<Auth0ProfileData>(entity =>
        {
            entity.ToTable("auth0_profile_data");
            entity.HasIndex(e => e.Email);
        });

        // Configure relationships
        modelBuilder.Entity<Auth0Identity>()
            .HasOne(i => i.ProfileData)
            .WithMany(p => p.Identities)
            .HasForeignKey(i => i.ProfileDataId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Auth0User>()
            .HasMany(u => u.Identities)
            .WithOne()
            .HasForeignKey(i => i.Auth0UserId)
            .HasPrincipalKey(u => u.Auth0UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Event model
        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("events");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .UseIdentityColumn();
            entity.Property(e => e.StartDate)
                .IsRequired();
            entity.Property(e => e.Name)
                .IsRequired();
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.StartDate);
        });
    }
}