using ESOF.WebApp.DBLayer.Context;
using ESOF.WebApp.DBLayer.Entities;
using ESOF.WebApp.DBLayer.Helpers;
using Helpers.Models;
using Microsoft.AspNetCore.Mvc;
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

app.MapPost("/talent_profiles", async (
    string profile_name,
    string country,
    string email,
    float price,
    float privacy,
    int fk_user_id,
    ApplicationDbContext db) =>
{
    var talentProfile = new TalentProfile
    {
        profile_name = profile_name,
        country = country,
        email = email,
        price = price,
        privacy = privacy,
        fk_user_id = fk_user_id,
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

app.MapDelete("/delete_user/{username}", async (string username, ApplicationDbContext db) =>
{
    // Find user by username
    var user = await db.Users.FirstOrDefaultAsync(u => u.username == username);
    if (user == null)
    {
        return Results.NotFound("User not found.");
    }

    // Remove user from database
    db.Users.Remove(user);
    await db.SaveChangesAsync();

    return Results.Ok("User deleted successfully.");
});

app.MapPut("/update_user/{username}", async (string username, string? newPassword, int? newRoleId, ApplicationDbContext db) =>
{
    // Find user by username
    var user = await db.Users.FirstOrDefaultAsync(u => u.username == username);
    if (user == null)
    {
        return Results.NotFound("User not found.");
    }

    // Update password if provided
    if (!string.IsNullOrEmpty(newPassword))
    {
        PasswordHelper.CreatePasswordHash(newPassword, out byte[] passwordHash, out byte[] passwordSalt);
        user.passwordHash = passwordHash;
        user.passwordSalt = passwordSalt;
    }

    // Update role if provided
    if (newRoleId.HasValue)
    {
        user.fk_role_id = newRoleId.Value;
    }

    await db.SaveChangesAsync();
    return Results.Ok("User updated successfully.");
});

app.MapPost("/logout", () =>
{
   
    return Results.Ok("User logged out successfully.");
});

app.MapGet("/get_user/{username}", async (string username, ApplicationDbContext db) =>
{
    // Find user by username
    var user = await db.Users.FirstOrDefaultAsync(u => u.username == username);
    if (user == null)
    {
        return Results.NotFound("User not found.");
    }

    // Return user info
    return Results.Ok(new
    {
        user.user_id,
        user.username,
        user.fk_role_id
    });
});

app.MapGet("/get_all_users", async (ApplicationDbContext db) =>
{
    var users = await db.Users
        .Join(db.Roles, 
            u => u.fk_role_id,   // fk_role_id da tabela Users
            r => r.role_id,           // id da tabela Roles
            (u, r) => new        // Resultado do Join
            {
                u.user_id,
                u.username,
                u.fk_role_id,
                RoleName = r.role  
            })
        .ToListAsync();

    return Results.Ok(users);
});

app.MapDelete("/delete_user_by_id/{id:int}", async (int id, ApplicationDbContext db) =>
{
    // Encontrar usuário pelo ID
    var user = await db.Users.FindAsync(id);
    if (user == null)
    {
        return Results.NotFound("User not found.");
    }

    // Remover usuário do banco de dados
    db.Users.Remove(user);
    await db.SaveChangesAsync();

    return Results.Ok($"User with ID {id} deleted successfully.");
});

app.MapPut("/update_user/{id:int}", async (int id, string? newPassword, int? newRoleId, ApplicationDbContext db) =>
{
    // Localiza o usuário pelo ID
    var user = await db.Users.FirstOrDefaultAsync(u => u.user_id == id);
    if (user == null)
    {
        return Results.NotFound("Usuário não encontrado.");
    }

    // Atualiza a senha, se fornecida
    if (!string.IsNullOrEmpty(newPassword))
    {
        PasswordHelper.CreatePasswordHash(newPassword, out byte[] passwordHash, out byte[] passwordSalt);
        user.passwordHash = passwordHash;
        user.passwordSalt = passwordSalt;
    }

    // Atualiza a role, se fornecida
    if (newRoleId.HasValue)
    {
        user.fk_role_id = newRoleId.Value;
    }

    await db.SaveChangesAsync();
    return Results.Ok("Usuário atualizado com sucesso.");
});


// Endpoint to create a skill
app.MapPost("/skills", async ([FromBody] Skill skill, ApplicationDbContext db) =>
{
    db.Skills.Add(skill);
    await db.SaveChangesAsync();

    return Results.Ok($"Skill '{skill.name}' criada com sucesso!");
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
app.MapPut("/skills/{id}", async (int id, [FromBody] Skill updatedSkill, ApplicationDbContext db) =>
{
    var skill = await db.Skills.FindAsync(id);
    if (skill == null)
        return Results.NotFound("Skill não encontrada!");

    // Atualiza os campos
    skill.name = updatedSkill.name;
    skill.area = updatedSkill.area;

    await db.SaveChangesAsync();
    return Results.Ok($"Skill {id} atualizada com sucesso!");
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
app.MapPost("/work_proposals", async (string proposalName, string category, string necessarySkills, int yearsOfExperience, string description, int totalHours, int fkUserId, ApplicationDbContext db) =>
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
app.MapPut("/work_proposal/{id}", async (int id, string proposalName, string category, string necessarySkills, int yearsOfExperience, string description, int totalHours, int fkUserId, ApplicationDbContext db) =>
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