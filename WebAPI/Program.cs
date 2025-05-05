using ESOF.WebApp.DBLayer.Context;
using ESOF.WebApp.DBLayer.Entities;
using ESOF.WebApp.DBLayer.Helpers;
using Microsoft.AspNetCore.Mvc;
using ESOF.WebApp.Services.Reports;
using ESOF.WebApp.WebAPI.DTOs;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Register ApplicationDbContext with the DI container
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<CategoryCountryReport>();
builder.Services.AddScoped<SkillReport>();
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

//TALENT PROFILES 
app.MapGet("/talent_profiles/list", async (ApplicationDbContext db) =>
{
    var publicProfiles = await db.TalentProfiles
        .Where(p => p.privacy == 0)
        .Select(p => new TalentProfileDto
        {
            ProfileId = p.profile_id,
            ProfileName = p.profile_name,
            Country = p.country,
            Email = p.email,
            Price = p.price,
            Privacy = p.privacy,
            Category = p.category,
            FkUserId = p.fk_user_id,
            Skills = p.TalentProfileSkills.Select(s => new SkillDto
            {
                SkillId = s.SkillId,
                SkillName = s.Skill.name,
                YearsOfExperience = s.YearsOfExperience
            }).ToList()
        })
        .ToListAsync();

    return Results.Ok(publicProfiles);
});



app.MapGet("/talent_profiles/{id}/list", async (int id, ApplicationDbContext db) =>
{
    var profile = await db.TalentProfiles
        .Include(p => p.TalentProfileSkills)
        .ThenInclude(tps => tps.Skill)
        .FirstOrDefaultAsync(p => p.profile_id == id);

    if (profile == null)
        return Results.NotFound();

    if (profile.privacy != 0)
        return Results.Unauthorized();

    return Results.Ok(profile);
});

app.MapPost("/talent_profile/add_profile", async (
    [FromQuery] string profile_name,
    [FromQuery] string country,
    [FromQuery] string email,
    [FromQuery] float price,
    [FromQuery] int privacy,
    [FromQuery] string category,
    [FromQuery] int fk_user_id,
    [FromServices] ApplicationDbContext db) =>
{
    var talentProfile = new TalentProfile
    {
        profile_name = profile_name,
        country = country,
        email = email,
        price = price,
        privacy = privacy,
        category = category,
        fk_user_id = fk_user_id
    };

    db.TalentProfiles.Add(talentProfile);
    await db.SaveChangesAsync();

    return Results.Created($"/talent_profiles/{talentProfile.profile_id}", talentProfile);
});

app.MapPost("/talent_profiles/{profile_name}/add_skill", async (
    string profile_name,
    [FromQuery] string skill_name,
    [FromQuery] int years_of_experience,
    [FromServices] ApplicationDbContext db) =>
{
    var profile = await db.TalentProfiles
        .Include(p => p.TalentProfileSkills)
        .FirstOrDefaultAsync(p => p.profile_name == profile_name);

    if (profile == null)
        return Results.NotFound($"Talent profile with name '{profile_name}' not found.");

    var skill = await db.Skills.FirstOrDefaultAsync(s => s.name == skill_name);
    if (skill == null)
        return Results.BadRequest($"Skill '{skill_name}' does not exist.");

    // Check if this skill is already added (optional)
    var alreadyExists = profile.TalentProfileSkills.Any(ts => ts.SkillId == skill.skill_id);
    if (alreadyExists)
        return Results.BadRequest($"Skill '{skill_name}' is already associated with the profile.");

    profile.TalentProfileSkills.Add(new TalentProfileSkill
    {
        TalentProfileId = profile.profile_id,
        SkillId = skill.skill_id,
        YearsOfExperience = years_of_experience
    });

    await db.SaveChangesAsync();
    return Results.Ok($"Skill '{skill_name}' added to profile '{profile_name}'.");
});

app.MapPut("/talent_profiles/{profile_name}/update", async (
    string profile_name,
    [FromQuery] string? country,
    [FromQuery] string? email,
    [FromQuery] float? price,
    [FromQuery] int? privacy,
    [FromQuery] string? category,
    [FromQuery] int? fk_user_id,
    [FromServices] ApplicationDbContext db) =>
{
    var profile = await db.TalentProfiles.FirstOrDefaultAsync(p => p.profile_name == profile_name);
    if (profile == null)
        return Results.NotFound($"Profile '{profile_name}' not found.");

    // Only update fields that were provided
    if (country != null) profile.country = country;
    if (email != null) profile.email = email;
    if (price != null) profile.price = price.Value;
    if (privacy != null) profile.privacy = privacy.Value;
    if (category != null) profile.category = category;
    if (fk_user_id != null) profile.fk_user_id = fk_user_id.Value;

    await db.SaveChangesAsync();
    return Results.Ok($"Profile '{profile_name}' updated.");
});

app.MapPut("/talent_profiles/{profile_name}/update_skill", async (
    string profile_name,
    [FromQuery] string skill_name,
    [FromQuery] int years_of_experience,
    [FromServices] ApplicationDbContext db) =>
{
    var profile = await db.TalentProfiles
        .Include(p => p.TalentProfileSkills)
        .ThenInclude(tps => tps.Skill)
        .FirstOrDefaultAsync(p => p.profile_name == profile_name);

    if (profile == null)
        return Results.NotFound($"Profile '{profile_name}' not found.");

    var skill = await db.Skills.FirstOrDefaultAsync(s => s.name == skill_name);
    if (skill == null)
        return Results.BadRequest($"Skill '{skill_name}' does not exist.");

    var skillEntry = profile.TalentProfileSkills.FirstOrDefault(ts => ts.SkillId == skill.skill_id);
    if (skillEntry == null)
        return Results.BadRequest($"Skill '{skill_name}' is not associated with the profile.");

    skillEntry.YearsOfExperience = years_of_experience;
    await db.SaveChangesAsync();

    return Results.Ok($"Updated '{skill_name}' to {years_of_experience} years for profile '{profile_name}'.");
});

app.MapDelete("/talent_profiles/{profile_name}/remove_skill", async (
    string profile_name,
    [FromQuery] string skill_name,
    [FromServices] ApplicationDbContext db) =>
{
    var profile = await db.TalentProfiles
        .Include(p => p.TalentProfileSkills)
        .FirstOrDefaultAsync(p => p.profile_name == profile_name);

    if (profile == null)
        return Results.NotFound($"Profile '{profile_name}' not found.");

    var skill = await db.Skills.FirstOrDefaultAsync(s => s.name == skill_name);
    if (skill == null)
        return Results.BadRequest($"Skill '{skill_name}' does not exist.");

    var skillEntry = profile.TalentProfileSkills.FirstOrDefault(ts => ts.SkillId == skill.skill_id);
    if (skillEntry == null)
        return Results.BadRequest($"Skill '{skill_name}' is not associated with the profile.");

    profile.TalentProfileSkills.Remove(skillEntry);
    await db.SaveChangesAsync();

    return Results.Ok($"Skill '{skill_name}' removed from profile '{profile_name}'.");
});

app.MapDelete("/talent_profiles/{profile_name}/delete", async (
    string profile_name,
    [FromServices] ApplicationDbContext db) =>
{
    var profile = await db.TalentProfiles
        .Include(p => p.TalentProfileSkills)
        .FirstOrDefaultAsync(p => p.profile_name == profile_name);

    if (profile == null)
        return Results.NotFound($"Talent profile '{profile_name}' not found.");

    db.TalentProfiles.Remove(profile);
    await db.SaveChangesAsync();

    return Results.Ok($"Talent profile '{profile_name}' and associated skills have been deleted.");
});

app.MapGet("/talent_profiles/search_by_skills", async (
    [FromQuery] string skill_names, // Recebe como string
    [FromServices] ApplicationDbContext db) =>
{
    if (string.IsNullOrEmpty(skill_names))
        return Results.BadRequest("At least one skill name must be provided.");

    // Converte a string separada por vírgulas para uma lista de skills
    var skillList = skill_names.Split(',').ToList();

    // Busca os talentos que têm todas as skills informadas
    var profiles = await db.TalentProfiles
        .Where(p => p.privacy == 0)
        .Include(p => p.TalentProfileSkills)
        .ThenInclude(tps => tps.Skill)
        .ToListAsync();

    // Filtra os perfis que têm todas as skills da busca
    var filtered = profiles
        .Where(p => skillList.All(sn =>
            p.TalentProfileSkills.Any(tps => tps.Skill.name.ToLower() == sn.ToLower())))
        .OrderBy(p => p.profile_name)
        .Select(p => new TalentProfileDto
        {
            ProfileId = p.profile_id,
            ProfileName = p.profile_name,
            Country = p.country,
            Email = p.email,
            Price = p.price,
            Privacy = p.privacy,
            Category = p.category,
            FkUserId = p.fk_user_id,
            Skills = p.TalentProfileSkills.Select(s => new SkillDto
            {
                SkillId = s.SkillId,
                SkillName = s.Skill.name,
                YearsOfExperience = s.YearsOfExperience
            }).ToList()
        })
        .ToList();

    return Results.Ok(filtered);
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
    var skill = await db.Skills
        .Include(s => s.UserSkills)
        .FirstOrDefaultAsync(s => s.skill_id == id);

    if (skill == null)
        return Results.NotFound("Skill not found.");

    if (skill.UserSkills.Any())
        return Results.BadRequest("Cannot delete skill because it is associated with one or more professionals.");

    db.Skills.Remove(skill);
    await db.SaveChangesAsync();

    return Results.NoContent();
});



// CREATE WORK PROPOSAL
app.MapPost("/work_proposals", async (WorkProposal workProposal, ApplicationDbContext db) =>
{
    if (workProposal == null)
    {
        return Results.BadRequest("Proposal data is missing.");
    }

    try
    {
        db.WorkProposals.Add(workProposal);
        await db.SaveChangesAsync();
        return Results.Created($"/work_proposals/{workProposal.proposal_id}", workProposal);
    }
    catch (Exception ex)
    {
        // Log any error that occurs
        Console.WriteLine($"Error creating proposal: {ex.Message}");
        return Results.StatusCode(500);
    }
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
app.MapPut("/work_proposals/{id}", async (int id, WorkProposal workProposal, ApplicationDbContext db) =>
{
    var existingProposal = await db.WorkProposals.FindAsync(id);
    if (existingProposal == null)
        return Results.NotFound("Proposal not found.");

    existingProposal.proposal_name = workProposal.proposal_name;
    existingProposal.category = workProposal.category;
    existingProposal.necessary_skills = workProposal.necessary_skills;
    existingProposal.years_of_experience = workProposal.years_of_experience;
    existingProposal.description = workProposal.description;
    existingProposal.total_hours = workProposal.total_hours;
    existingProposal.fk_user_id = workProposal.fk_user_id;

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

//REPORTS

app.MapGet("/reports/category-country", async (ApplicationDbContext db) =>
    {
        var report = new CategoryCountryReport(db).GenerateReport();
        return Results.Ok(report);
    })
    .WithName("GetCategoryCountryReport")
    .WithSummary("Obtém o preço médio mensal por categoria e país.")
    .WithDescription("Baseado em 176 horas por mês, agrupa talentos por categoria e país.")
    .Produces<Dictionary<string, float>>(StatusCodes.Status200OK);

app.MapGet("/reports/skill", async (ApplicationDbContext db) =>
    {
        var report = new SkillReport(db).GenerateReport();
        return Results.Ok(report);
    })
    .WithName("GetSkillReport")
    .WithSummary("Obtém o preço médio mensal por skill.")
    .WithDescription("Baseado em 176 horas por mês, agrupa talentos pelas suas skills.")
    .Produces<Dictionary<string, float>>(StatusCodes.Status200OK);


//EXPERIENCES



app.UseHttpsRedirection();

app.Run();