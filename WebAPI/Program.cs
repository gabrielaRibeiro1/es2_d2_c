using ESOF.WebApp.DBLayer.Context;
using ESOF.WebApp.DBLayer.Entities;
using ESOF.WebApp.DBLayer.Helpers;
using Helpers.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Register ApplicationDbContext with the DI container
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
    c =>
    {
        c.SupportNonNullableReferenceTypes();
    });

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Endpoint to create a new user
app.MapPost("/create_account", async (string username, string password, int fk_role_id, ApplicationDbContext db) =>
{
    if (string.IsNullOrEmpty(password))
    {
        return Results.BadRequest("Password cannot be empty.");
    }

    PasswordHelper.CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

    if (passwordHash == null || passwordSalt == null)
    {
        return Results.BadRequest("Error generating password hash and salt.");
    }

    var newUser = new User
    {
        username = username,
        passwordHash = passwordHash,
        passwordSalt = passwordSalt,
        fk_role_id = fk_role_id
    };

    db.Users.Add(newUser);
    await db.SaveChangesAsync();

    return Results.Created($"/users/{newUser.user_id}", newUser);
});

// Endpoint for user login
app.MapPost("/login", async (string username, string password, ApplicationDbContext db) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.username == username);
    if (user == null || !PasswordHelper.VerifyPassword(password, user.passwordHash, user.passwordSalt))
    {
        return Results.Unauthorized();
    }

    return Results.Ok(new
    {
        user.user_id,
        user.username,
        user.fk_role_id
    });
});

// Endpoint to create a skill
app.MapPost("/skills", async (string name, string area, ApplicationDbContext db) => 
{
    var skill = new Skill 
    {
        name = name,
        area = area
    };

    db.Skills.Add(skill);
    await db.SaveChangesAsync();

    return Results.Ok($"Skill '{name}' criada com sucesso!");
});

// Endpoint to get all skills
app.MapGet("/skills", async (ApplicationDbContext db) =>
{
    return await db.Skills.Select(s => new { s.skill_id, s.name, s.area }).ToListAsync();
});

// Endpoint to get a skill by ID
app.MapGet("/skills/{id}", async (int id, ApplicationDbContext db) =>
{
    var skill = await db.Skills.Where(s => s.skill_id == id)
                               .Select(s => new { s.skill_id, s.name, s.area })
                               .FirstOrDefaultAsync();
    return skill == null ? Results.NotFound() : Results.Ok(skill);
});

// Endpoint to update a skill
app.MapPut("/skills/{id}", async (
    int id,                      
    string name,                 
    string area,                 
    ApplicationDbContext db
) =>
{
    var skill = await db.Skills.FindAsync(id);
    if (skill == null)
        return Results.NotFound("Skill não encontrada!");

    // Update name and area
    skill.name = name;
    skill.area = area;

    await db.SaveChangesAsync();
    return Results.Ok($"Skill {id} atualizada: Nome='{name}', Área='{area}'");
});

// Endpoint to delete a skill
app.MapDelete("/skills/{id}", async (int id, ApplicationDbContext db) =>
{
    var skill = await db.Skills.FindAsync(id);
    if (skill == null)
        return Results.NotFound();

    db.Skills.Remove(skill);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();
