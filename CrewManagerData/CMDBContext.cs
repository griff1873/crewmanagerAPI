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
    public DbSet<EventType> EventTypes { get; set; } = null!;
    public DbSet<Boat> Boats { get; set; } = null!;
    public DbSet<BoatCrew> BoatCrews { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // This is only used for design-time operations (migrations)
            // Try multiple paths to find appsettings.json
            var basePaths = new[]
            {
                AppDomain.CurrentDomain.BaseDirectory,
                Directory.GetCurrentDirectory(),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "CrewManagerAPI"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "CrewManagerAPI")
            };

            IConfigurationRoot? configuration = null;

            foreach (var basePath in basePaths)
            {
                try
                {
                    var configPath = Path.Combine(basePath, "appsettings.json");
                    if (File.Exists(configPath))
                    {
                        configuration = new ConfigurationBuilder()
                            .SetBasePath(basePath)
                            .AddJsonFile("appsettings.json", optional: false)
                            .Build();
                        break;
                    }
                }
                catch
                {
                    // Continue to next path
                }
            }

            if (configuration != null)
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                if (!string.IsNullOrEmpty(connectionString))
                {
                    optionsBuilder.UseNpgsql(connectionString);
                }
            }
        }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure existing models
        modelBuilder.Entity<Profile>(entity =>
        {
            entity.ToTable("profiles");
            entity.Property(p => p.LoginId)
                .IsRequired();
            entity.Property(p => p.Name)
                .IsRequired();
            entity.Property(p => p.Email)
                .IsRequired();
            entity.HasIndex(e => e.LoginId).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

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
            entity.Property(e => e.BoatId)
                .IsRequired();
            entity.Property(e => e.EventTypeId)
                .IsRequired();
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.StartDate);
            entity.HasIndex(e => e.BoatId);
            entity.HasIndex(e => e.EventTypeId);
        });

        // Configure EventType model
        modelBuilder.Entity<EventType>(entity =>
        {
            entity.ToTable("event_type");
            entity.HasKey(et => et.Id);
            entity.Property(et => et.Id)
                .ValueGeneratedOnAdd()
                .UseIdentityColumn();
            entity.Property(et => et.Name)
                .IsRequired()
                .HasMaxLength(200);
            entity.HasIndex(et => et.Name);
            entity.HasIndex(et => et.ProfileId);
        });

        // Configure Boat model
        modelBuilder.Entity<Boat>(entity =>
        {
            entity.ToTable("boats");
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Id)
                .ValueGeneratedOnAdd()
                .UseIdentityColumn();
            entity.Property(b => b.Name)
                .IsRequired();
            entity.Property(b => b.ProfileId)
                .IsRequired();
            entity.HasIndex(b => b.Name);
            entity.HasIndex(b => b.ProfileId);
        });

        // Configure Boat-Event relationship
        modelBuilder.Entity<Event>()
            .HasOne(e => e.Boat)
            .WithMany(b => b.Events)
            .HasForeignKey(e => e.BoatId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent deleting boat if it has events

        // Configure EventType-Event relationship
        modelBuilder.Entity<Event>()
            .HasOne(e => e.EventType)
            .WithMany()
            .HasForeignKey(e => e.EventTypeId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent deleting event type if it has events

        // Configure BoatCrew model
        modelBuilder.Entity<BoatCrew>(entity =>
        {
            entity.ToTable("boat_crew");
            entity.HasKey(bc => bc.Id);
            entity.Property(bc => bc.Id)
                .ValueGeneratedOnAdd()
                .UseIdentityColumn();
            entity.Property(bc => bc.ProfileId)
                .IsRequired();
            entity.Property(bc => bc.BoatId)
                .IsRequired();

            // Composite unique index to prevent duplicate crew assignments
            entity.HasIndex(bc => new { bc.ProfileId, bc.BoatId }).IsUnique();
            entity.HasIndex(bc => bc.ProfileId);
            entity.HasIndex(bc => bc.BoatId);
        });

        // Configure Profile-Boat relationship (ownership)
        modelBuilder.Entity<Boat>()
            .HasOne(b => b.Profile)
            .WithMany(p => p.Boats)
            .HasForeignKey(b => b.ProfileId)
            .OnDelete(DeleteBehavior.Cascade); // Delete boats when profile is deleted

        // Configure Profile-BoatCrew relationship
        modelBuilder.Entity<BoatCrew>()
            .HasOne(bc => bc.Profile)
            .WithMany(p => p.BoatCrews)
            .HasForeignKey(bc => bc.ProfileId)
            .OnDelete(DeleteBehavior.Cascade); // Remove crew assignments when profile is deleted

        // Configure Boat-BoatCrew relationship
        modelBuilder.Entity<BoatCrew>()
            .HasOne(bc => bc.Boat)
            .WithMany(b => b.BoatCrews)
            .HasForeignKey(bc => bc.BoatId)
            .OnDelete(DeleteBehavior.Cascade); // Remove crew assignments when boat is deleted
    }
}