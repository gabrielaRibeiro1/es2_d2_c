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
    
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Ingredient> Ingredients { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderPizza> OrderPizzas { get; set; }
    public DbSet<Pizza> Pizzas { get; set; }
    public DbSet<PizzaIngredient> PizzaIngredients { get; set; }
    
    public DbSet<Experience> Experience { get; set; }
    public DbSet<ProfileSkill> ProfileSkills { get; set; }
    public DbSet<Role> Role { get; set; }
    public DbSet<Skills> Skills { get; set; }
    public DbSet<TalentProfile> TalentProfile { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<WorkProposals> WorkProposal { get; set; }
    public DbSet<WorkSkill> WorkSkill { get; set; }

    

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {


        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleID);
           

        // User - TalentProfile (One-to-Many)
        modelBuilder.Entity<User>()
            .HasMany(u => u.TalentProfile)
            .WithOne(tp => tp.User)
            .HasForeignKey(tp => tp.UserID);

        // User - WorkProposals (One-to-Many)
        modelBuilder.Entity<User>()
            .HasMany(u => u.WorkProposals)
            .WithOne(wp => wp.User)
            .HasForeignKey(wp => wp.UserID);

        // TalentProfile - Experience (One-to-Many)
        modelBuilder.Entity<TalentProfile>()
            .HasMany(tp => tp.Experience)
            .WithOne(e => e.TalentProfile)
            .HasForeignKey(e => e.ProfileID);
         
        // TalentProfile - ProfileSkill (One-to-Many)
        modelBuilder.Entity<TalentProfile>()
            .HasMany(tp => tp.ProfileSkill)
            .WithOne(ps => ps.TalentProfile)
            .HasForeignKey(ps => ps.ProfileID);
          

        // ProfileSkill - Skills (Many-to-One)
        modelBuilder.Entity<ProfileSkill>()
            .HasOne(ps => ps.Skills)
            .WithMany() // Skills does not have ICollection<ProfileSkill>
            .HasForeignKey(ps => ps.SkillID);
           

        // WorkProposals - WorkSkill (One-to-Many)
        modelBuilder.Entity<WorkProposals>()
            .HasOne(wp => wp.WorkSkill)
            .WithMany()
            .HasForeignKey(wp => wp.WorkSkillID);


        modelBuilder.Entity<WorkSkill>()
            .HasOne(ws => ws.Skills)
            .WithMany() // Skills does not have ICollection<WorkSkill>
            .HasForeignKey(ws => ws.SkillID);
           

        // WorkSkill - WorkProposals (Many-to-One)
        modelBuilder.Entity<WorkSkill>()
            .HasOne(ws => ws.WorkProposals)
            .WithMany()
            .HasForeignKey(ws => ws.ProposalID);
           

        // Skills - WorkSkill (One-to-Many)
        modelBuilder.Entity<Skills>()
            .HasOne(s => s.WorkSkill)
            .WithMany()
            .HasForeignKey(s => s.WorkSkillID);
           

        // Skills - ProfileSkill (One-to-Many)
        modelBuilder.Entity<Skills>()
            .HasOne(s => s.ProfileSkill)
            .WithMany()
            .HasForeignKey(s => s.ProfileSkillID);


    }
    
}
