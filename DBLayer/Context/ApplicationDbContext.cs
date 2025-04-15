using DotNetEnv;
using ESOF.WebApp.DBLayer.Entities;
using Helpers;
using Microsoft.EntityFrameworkCore;

namespace ESOF.WebApp.DBLayer.Context;

public partial class ApplicationDbContext : DbContext
{
    static ApplicationDbContext()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }
    
    private static readonly DbContextOptions DefaultOptions = new Func<DbContextOptions>(() =>
    {
        var optionsBuilder = new DbContextOptionsBuilder();
        var db = EnvFileHelper.GetString("POSTGRES_DB");
        var user = EnvFileHelper.GetString("POSTGRES_USER");
        var password = EnvFileHelper.GetString("POSTGRES_PASSWORD");
        var port = EnvFileHelper.GetString("POSTGRES_PORT");
        var host = EnvFileHelper.GetString("POSTGRES_HOST");

        if (string.IsNullOrEmpty(db) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password) ||
            string.IsNullOrEmpty(port) || string.IsNullOrEmpty(host))
        {
            throw new InvalidOperationException(
                "Database connection information not fully specified in environment variables.");
        }

        var connectionString = $"Host={host};Port={port};Database={db};Username={user};Password={password}";
        optionsBuilder.UseNpgsql(connectionString);
        return optionsBuilder.Options;
    })();
    
    public ApplicationDbContext()
        : base(DefaultOptions)
    {
    }
    

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSet properties representing tables
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Skill> Skills { get; set; }
    public DbSet<TalentProfile> TalentProfiles { get; set; }
    public DbSet<UserSkill> UserSkills { get; set; }
    public DbSet<WorkProposal> WorkProposals { get; set; }
    public DbSet<Experience> Experiences { get; set; }

    // OnModelCreating for configuring relationships and keys
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // USERSKILL
        modelBuilder.Entity<UserSkill>()
            .HasKey(us => new { us.UserId, us.SkillId });

        modelBuilder.Entity<UserSkill>()
            .HasOne(u => u.User)
            .WithMany(us => us.UserSkills)
            .HasForeignKey(fk => fk.UserId);

        modelBuilder.Entity<UserSkill>()
            .HasOne(s => s.Skill)
            .WithMany(su => su.UserSkills)
            .HasForeignKey(fk => fk.SkillId);

        // USER
        modelBuilder.Entity<User>()
            .HasKey(u => u.user_id);
        
        modelBuilder.Entity<User>()
            .Property(u => u.passwordHash)
            .HasColumnType("bytea")
            .IsRequired();

        modelBuilder.Entity<User>()
            .Property(u => u.passwordSalt)
            .HasColumnType("bytea")
            .IsRequired();

        modelBuilder.Entity<User>()
            .Property(u => u.user_id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.fk_role_id);

        modelBuilder.Entity<User>()
            .HasMany(u => u.Proposals)
            .WithOne(wp => wp.User)
            .HasForeignKey(wp => wp.fk_user_id);

        // One-to-many relationship: User has many TalentProfiles
        modelBuilder.Entity<User>()
            .HasMany(u => u.Profiles)
            .WithOne(tp => tp.User)
            .HasForeignKey(tp => tp.fk_user_id);

        // One-to-many relationship: User has many UserSkills
        modelBuilder.Entity<User>()
            .HasMany(u => u.UserSkills)
            .WithOne(us => us.User)
            .HasForeignKey(us => us.UserId);

        // SKILL
        modelBuilder.Entity<Skill>()
            .HasKey(s => s.skill_id);
        
        modelBuilder.Entity<Skill>()
            .Property(u => u.skill_id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<Skill>()
            .HasMany(s => s.UserSkills)
            .WithOne(us => us.Skill)
            .HasForeignKey(us => us.SkillId);

        // ROLE
        modelBuilder.Entity<Role>()
            .HasKey(r => r.role_id);
        
        modelBuilder.Entity<Role>()
            .Property(u => u.role_id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<Role>()
            .HasMany(r => r.Users)
            .WithOne(u => u.Role)
            .HasForeignKey(u => u.fk_role_id)
            .OnDelete(DeleteBehavior.Restrict);

        // TALENTPROFILE
        modelBuilder.Entity<TalentProfile>()
            .HasKey(tp => tp.profile_id);
        
        modelBuilder.Entity<TalentProfile>()
            .Property(u => u.profile_id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<TalentProfile>()
            .HasOne(tp => tp.User)
            .WithMany(u => u.Profiles)
            .HasForeignKey(tp => tp.fk_user_id)
            .HasConstraintName("FK_TalentProfiles_Users_fk_user_id")
            .OnDelete(DeleteBehavior.Restrict);

        // EXPERIENCE
        modelBuilder.Entity<Experience>()
            .HasKey(e => e.experience_id);
        
        modelBuilder.Entity<Experience>()
            .Property(u => u.experience_id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<Experience>()
            .HasOne(e => e.Profile)
            .WithMany(tp => tp.Experiences)
            .HasForeignKey(e => e.fk_profile_id)
            .OnDelete(DeleteBehavior.Cascade);
    }

}