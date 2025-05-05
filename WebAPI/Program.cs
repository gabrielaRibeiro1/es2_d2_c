using ESOF.WebApp.DBLayer.Context;
using ESOF.WebApp.DBLayer.Entities;
using ESOF.WebApp.DBLayer.Helpers;
using Helpers.Models;
using Microsoft.EntityFrameworkCore;
using ESOF.WebApp.DBLayer.DTOs; // importa o DTO
using Microsoft.AspNetCore.Mvc; // necessário para [FromBody]



var builder = WebApplication.CreateBuilder(args);

// Register ApplicationDbContext with the DI container
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/create_account", async (string username, string password, int fk_role_id, ApplicationDbContext db) =>
{
    // Step 1: Check if the password is provided
    if (string.IsNullOrEmpty(password))
    {
        return Results.BadRequest("Password cannot be empty.");
    }

    // Step 2: Generate the password hash and salt
    PasswordHelper.CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

    // Step 3: Ensure passwordHash and passwordSalt are not null
    if (passwordHash == null || passwordSalt == null)
    {
        return Results.BadRequest("Error generating password hash and salt.");
    }

    // Step 4: Create the new user without storing the raw password, but with the hashed password and salt
    var newUser = new User
    {
        username = username,  // Use the username directly
        passwordHash = passwordHash, // Store hashed password (byte[])
        passwordSalt = passwordSalt, // Store salt (byte[])
        fk_role_id = fk_role_id  // Use fk_role_id directly
    };

    // Step 5: Save the new user to the database
    db.Users.Add(newUser);
    await db.SaveChangesAsync();

    return Results.Created($"/users/{newUser.user_id}", newUser);
});

app.MapPost("/login", async (string username, string password, ApplicationDbContext db) =>
{
    // Find user by username
    var user = await db.Users.FirstOrDefaultAsync(u => u.username == username);
    if (user == null)
        return Results.Unauthorized();

    // Verify the password
    if (!PasswordHelper.VerifyPassword(password, user.passwordHash, user.passwordSalt))
        return Results.Unauthorized();

    // Return user info upon successful login
    return Results.Ok(new
    {
        user.user_id,
        user.username,
        user.fk_role_id
    });
});

// CRUD TalentProfiles
app.MapGet("/talent_profiles", async (ApplicationDbContext db) =>
{
    var talentProfiles = await db.TalentProfiles.ToListAsync();
    return Results.Ok(talentProfiles);
});

app.MapGet("/talent_profiles/{id}", async (int id, ApplicationDbContext db) =>
{
    var talentProfile = await db.TalentProfiles.FindAsync(id);
    return talentProfile == null ? Results.NotFound() : Results.Ok(talentProfile);
});

app.MapPost("/talent_profiles", async ([FromBody] TalentProfileDto dto, ApplicationDbContext db) =>
{
    var talentProfile = new TalentProfile
    {
        profile_name = dto.profile_name,
        country = dto.country,
        email = dto.email,
        price = dto.price,
        privacy = dto.privacy,
        fk_user_id = dto.fk_user_id
    };

    db.TalentProfiles.Add(talentProfile);
    await db.SaveChangesAsync();
    return Results.Created($"/talent_profiles/{talentProfile.profile_id}", talentProfile);
});

app.MapPut("/talent_profiles/{id}", async (
    int id,
    string profile_name,
    string country,
    string email,
    float price,
    float privacy,
    int fk_user_id,
    ApplicationDbContext db) =>
{
    var existingProfile = await db.TalentProfiles.FindAsync(id);
    if (existingProfile == null)
        return Results.NotFound("Perfil não encontrado.");

    // Atualiza apenas os campos permitidos
    existingProfile.profile_name = profile_name;
    existingProfile.country = country;
    existingProfile.email = email;
    existingProfile.price = price;
    existingProfile.privacy = privacy;
    existingProfile.fk_user_id = fk_user_id;


    await db.SaveChangesAsync();
    return Results.Ok(existingProfile);
});

app.MapDelete("/talent_profiles/{id}", async (int id, ApplicationDbContext db) =>
{
    var talentProfile = await db.TalentProfiles.FindAsync(id);
    if (talentProfile == null) return Results.NotFound();
    
    db.TalentProfiles.Remove(talentProfile);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.UseHttpsRedirection();

app.Run();