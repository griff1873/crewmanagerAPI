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
    public DbSet<Schedule> Schedules { get; set; } = null!;
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
            entity.HasIndex(e => e.LoginId).IsUnique();
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

        // Configure Schedule model
        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.ToTable("schedules");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Id)
                .ValueGeneratedOnAdd()
                .UseIdentityColumn();
            entity.Property(s => s.Name)
                .IsRequired();
            entity.Property(s => s.BoatId)
                .IsRequired();
            entity.HasIndex(s => s.Name);
            entity.HasIndex(s => s.BoatId);
        });

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
            entity.Property(e => e.ScheduleId)
                .IsRequired();
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.StartDate);
            entity.HasIndex(e => e.ScheduleId);
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
            entity.Property(b => b.ProfileLoginId)
                .IsRequired();
            entity.HasIndex(b => b.Name);
            entity.HasIndex(b => b.ProfileLoginId);
        });

        // Configure Schedule-Event relationship
        modelBuilder.Entity<Event>()
            .HasOne(e => e.Schedule)
            .WithMany(s => s.Events)
            .HasForeignKey(e => e.ScheduleId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent deleting schedule if it has events

        // Configure Boat-Schedule relationship
        modelBuilder.Entity<Schedule>()
            .HasOne(s => s.Boat)
            .WithMany(b => b.Schedules)
            .HasForeignKey(s => s.BoatId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent deleting boat if it has schedules

        // Configure BoatCrew model
        modelBuilder.Entity<BoatCrew>(entity =>
        {
            entity.ToTable("boat_crew");
            entity.HasKey(bc => bc.Id);
            entity.Property(bc => bc.Id)
                .ValueGeneratedOnAdd()
                .UseIdentityColumn();
            entity.Property(bc => bc.ProfileLoginId)
                .IsRequired();
            entity.Property(bc => bc.BoatId)
                .IsRequired();

            // Composite unique index to prevent duplicate crew assignments
            entity.HasIndex(bc => new { bc.ProfileLoginId, bc.BoatId }).IsUnique();
            entity.HasIndex(bc => bc.ProfileLoginId);
            entity.HasIndex(bc => bc.BoatId);
        });

        // Configure Profile-Boat relationship (ownership)
        modelBuilder.Entity<Boat>()
            .HasOne(b => b.Profile)
            .WithMany(p => p.Boats)
            .HasForeignKey(b => b.ProfileLoginId)
            .HasPrincipalKey(p => p.LoginId)
            .OnDelete(DeleteBehavior.Cascade); // Delete boats when profile is deleted

        // Configure Profile-BoatCrew relationship
        modelBuilder.Entity<BoatCrew>()
            .HasOne(bc => bc.Profile)
            .WithMany(p => p.BoatCrews)
            .HasForeignKey(bc => bc.ProfileLoginId)
            .HasPrincipalKey(p => p.LoginId)
            .OnDelete(DeleteBehavior.Cascade); // Remove crew assignments when profile is deleted

        // Configure Boat-BoatCrew relationship
        modelBuilder.Entity<BoatCrew>()
            .HasOne(bc => bc.Boat)
            .WithMany(b => b.BoatCrews)
            .HasForeignKey(bc => bc.BoatId)
            .OnDelete(DeleteBehavior.Cascade); // Remove crew assignments when boat is deleted
    }
}