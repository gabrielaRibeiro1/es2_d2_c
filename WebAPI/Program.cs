using ESOF.WebApp.DBLayer.Context;
using ESOF.WebApp.DBLayer.Entities;
using ESOF.WebApp.DBLayer.Helpers;
using Helpers.Models;
using Microsoft.EntityFrameworkCore;


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

// CREATE WORK PROPOSAL
app.MapPost("/work_proposals", async (string proposalName, string category, string necessarySkills, string yearsOfExperience, string description, string totalHours, int fkUserId, ApplicationDbContext db) =>
{
    var workProposal = new WorkProposal
    {
        proposal_name = proposalName,
        category = category,
        necessary_skills = necessarySkills,
        years_of_experience = yearsOfExperience,
        description = description,
        total_hours = totalHours,
        fk_user_id = fkUserId,
    };

    db.WorkProposals.Add(workProposal);
    await db.SaveChangesAsync();
    return Results.Created($"/work_proposals/{workProposal.proposal_id}", workProposal);
});

// GET ALL WORK PROPOSALS
app.MapGet("/work_proposals", async (ApplicationDbContext db) =>
{
    var workProposals = await db.WorkProposals.ToListAsync();
    return Results.Ok(workProposals);
});

// GET WORK PROPOSAL BY ID
app.MapGet("/work_proposal/{id}", async (int id, ApplicationDbContext db) =>
{
    var workProposal = await db.WorkProposals.FindAsync(id);
    return workProposal == null ? Results.NotFound() : Results.Ok(workProposal);
});

// UPDATE WORK PROPOSAL
app.MapPut("/work_proposal/{id}", async (int id, string proposalName, string category, string necessarySkills, string yearsOfExperience, string description, string totalHours, int fkUserId, ApplicationDbContext db) =>
{
    var existingProposal = await db.WorkProposals.FindAsync(id);
    if (existingProposal == null)
        return Results.NotFound("Perfil não encontrado.");

    existingProposal.proposal_name = proposalName;
    existingProposal.category = category;
    existingProposal.necessary_skills = necessarySkills;
    existingProposal.years_of_experience = yearsOfExperience;
    existingProposal.description = description;
    existingProposal.total_hours = totalHours;
    existingProposal.fk_user_id = fkUserId;


    await db.SaveChangesAsync();
    return Results.Ok(existingProposal);
});

// DELETE WORK PROPOSAL
app.MapDelete("/work_proposals/{id}", async (int id, ApplicationDbContext db) =>
{
    var workProposal = await db.WorkProposals.FindAsync(id);
    if (workProposal == null) return Results.NotFound();

    db.WorkProposals.Remove(workProposal);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.UseHttpsRedirection();

app.Run();