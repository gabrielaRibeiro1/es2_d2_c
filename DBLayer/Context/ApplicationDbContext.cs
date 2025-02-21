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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PizzaIngredient>()
            .HasKey(pi => new { pi.PizzaId, pi.IngredientId });

        modelBuilder.Entity<PizzaIngredient>()
            .HasOne(pi => pi.Pizza)
            .WithMany(p => p.PizzaIngredients)
            .HasForeignKey(pi => pi.PizzaId);

        modelBuilder.Entity<PizzaIngredient>()
            .HasOne(pi => pi.Ingredient)
            .WithMany(i => i.PizzaIngredients)
            .HasForeignKey(pi => pi.IngredientId);

        modelBuilder.Entity<OrderPizza>()
            .HasKey(op => new { op.OrderId, op.PizzaId });

        modelBuilder.Entity<OrderPizza>()
            .HasOne(op => op.Order)
            .WithMany(o => o.OrderPizzas)
            .HasForeignKey(op => op.OrderId);

        modelBuilder.Entity<OrderPizza>()
            .HasOne(op => op.Pizza)
            .WithMany(p => p.OrderPizzas)
            .HasForeignKey(op => op.PizzaId);
    }
    
}
