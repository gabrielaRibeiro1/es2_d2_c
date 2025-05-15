using System.Text.Json;
using ESOF.WebApp.DBLayer.Context;
using ESOF.WebApp.DBLayer.Entities;
using ESOF.WebApp.DBLayer.Helpers;
using Microsoft.AspNetCore.Mvc;
using ESOF.WebApp.Services.Reports;

using ESOF.WebApp.WebAPI.DTOs;

using Microsoft.EntityFrameworkCore;
using ESOF.WebApp.DBLayer.DTOs; // importa o DTO
using Microsoft.AspNetCore.Mvc; // necessário para [FromBody]



var builder = WebApplication.CreateBuilder(args);

// Register ApplicationDbContext with the DI container
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<CategoryCountryReport>();
builder.Services.AddScoped<SkillReport>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/create_account", async ([FromBody] UserAddModel request, ApplicationDbContext db) =>
{
    if (string.IsNullOrEmpty(request.Password))
        return Results.BadRequest("Password cannot be empty.");

    PasswordHelper.CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

    if (passwordHash == null || passwordSalt == null)
        return Results.BadRequest("Error generating password hash and salt.");

    var newUser = new User
    {
        username = request.Username,
        passwordHash = passwordHash,
        passwordSalt = passwordSalt,
        fk_role_id = request.RoleId
    };

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

app.MapGet("/get_user_by_id/{id:int}", 
    async (int id, ApplicationDbContext db) =>
    {
        // Tenta encontrar pelo PK (assume que user_id é a chave primária)
        var user = await db.Users.FindAsync(id);
        if (user == null)
        {
            return Results.NotFound(new { message = "User not found." });
        }

        // Retorna apenas os campos necessários
        return Results.Ok(new
        {
            user.user_id,
            user.username,
            user.fk_role_id
        });
    });

app.MapPost("/add_user", async (UserAddModel model, ApplicationDbContext db) =>
    {
        if (string.IsNullOrEmpty(model.Password))
            return Results.BadRequest("Password cannot be empty.");

        PasswordHelper.CreatePasswordHash(model.Password, out byte[] passwordHash, out byte[] passwordSalt);
        if (passwordHash == null || passwordSalt == null)
            return Results.BadRequest("Error generating password hash and salt.");

        var newUser = new User
        {
            username      = model.Username,
            passwordHash  = passwordHash,
            passwordSalt  = passwordSalt,
            fk_role_id    = model.RoleId
        };

        db.Users.Add(newUser);
        await db.SaveChangesAsync();

        return Results.Created($"/users/{newUser.user_id}", newUser);
    })
    .WithName("AddUser")
    .Accepts<UserAddModel>("application/json")
    .Produces<User>(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status400BadRequest);


app.MapPut("/update_user2/{id:int}", async (int id, UserUpdateModel updateData, ApplicationDbContext db) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.user_id == id);
    if (user == null)
    {
        return Results.NotFound("Usuário não encontrado.");
    }

    // Atualiza a senha, se fornecida
    if (!string.IsNullOrEmpty(updateData.Password))
    {
        PasswordHelper.CreatePasswordHash(updateData.Password, out byte[] passwordHash, out byte[] passwordSalt);
        user.passwordHash = passwordHash;
        user.passwordSalt = passwordSalt;
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

app.MapGet("/work_proposals/{proposalId}/eligible_talents", async (int proposalId, ApplicationDbContext db) => 
{
    // Retrieve the work proposal by ID
    var proposal = await db.WorkProposals
        .FirstOrDefaultAsync(p => p.proposal_id == proposalId);

    if (proposal == null)
    {
        return Results.NotFound("Work proposal not found.");
    }

    // Retrieve talents that are eligible for this proposal
    var eligibleTalents = await db.TalentProfiles
        .Where(p => p.category == proposal.category) // Filter by category
        .Include(p => p.TalentProfileSkills) // Include skills
        .Include(p => p.Experiences) // Include experiences
        .ToListAsync();

    // Calculate the total value of each talent based on experience years
    var talentDtos = eligibleTalents.Select(p => new TalentProfileDto
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
        }).ToList(),
        Experiences = p.Experiences.Select(e => new ExperienceDto
        {
            ExperienceId = e.experience_id,
            CompanyName = e.company_name,
            StartYear = e.start_year,
            EndYear = e.end_year // No null handling needed as end_year is non-nullable
        }).ToList(),
        TotalValue = p.Experiences.Sum(e =>
        {
            // Calculate the total value based on years worked and price
            int experienceYears = e.end_year - e.start_year; // end_year is not nullable, no need for ?? DateTime.Now.Year
            return experienceYears * p.price * 1000; // Adjust the multiplier as needed
        })
    })
    .OrderByDescending(p => p.TotalValue) // Order talents by their total value
    .ToList();

    return Results.Ok(talentDtos);
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
app.MapPost("/talent_profiles/{profile_name}/add_experience", async (
    string profile_name,
    [FromQuery] string company_name,
    [FromQuery] int start_year,
    [FromQuery] int? end_year,
    [FromServices] ApplicationDbContext db) =>
{
    var profile = await db.TalentProfiles
        .Include(p => p.Experiences)
        .FirstOrDefaultAsync(p => p.profile_name == profile_name);

    if (profile == null)
        return Results.NotFound($"Talent profile with name '{profile_name}' not found.");

    // If end_year was not provided, assume it's the same as start_year (still working)
    int resolvedEndYear = end_year.HasValue ? end_year.Value : start_year;

    // Check for overlapping years with existing experiences
    foreach (var existing in profile.Experiences)
    {
        int existingStart = existing.start_year;
        int existingEnd = existing.end_year; // always set since it's int, not int?

        bool overlaps = start_year <= existingEnd && resolvedEndYear >= existingStart;
        if (overlaps)
        {
            return Results.BadRequest($"The experience period {start_year}-{resolvedEndYear} overlaps with an existing experience ({existingStart}-{existingEnd}).");
        }
    }

    var experience = new Experience
    {
        company_name = company_name,
        start_year = start_year,
        end_year = resolvedEndYear,
        fk_profile_id = profile.profile_id
    };

    db.Experiences.Add(experience);
    await db.SaveChangesAsync();

    return Results.Created($"/experiences/{experience.experience_id}", new
    {
        experience.experience_id,
        experience.company_name,
        experience.start_year,
        experience.end_year,
        fk_profile_id = profile.profile_id
    });
});




app.MapGet("/experiences/by-profile", async (
    [FromQuery] string profileName,
    [FromServices] ApplicationDbContext dbContext) =>
{
    // Retrieve the TalentProfile by ProfileName from the query parameter
    var profile = await dbContext.TalentProfiles
        .FirstOrDefaultAsync(p => p.profile_name == profileName);

    if (profile == null)
    {
        return Results.NotFound("Profile not found");
    }

    // Retrieve all experiences related to the profile and project to anonymous objects
    var experiences = await dbContext.Experiences
        .Where(e => e.fk_profile_id == profile.profile_id)
        .Select(e => new
        {
            e.experience_id,
            e.company_name,
            e.start_year,
            e.end_year,
            profile_name = profile.profile_name
        })
        .ToListAsync();

    if (!experiences.Any())
    {
        return Results.NotFound("No experiences found for this profile");
    }

    return Results.Ok(experiences);
});


app.MapGet("/experiences", async (ApplicationDbContext dbContext) =>
{
    var experiences = await dbContext.Experiences.ToListAsync();

    return Results.Ok(experiences);
});

app.MapPut("/experiences/{id}", async (
    [FromRoute] int id,
    [FromQuery] string company_name,
    [FromQuery] int start_year,
    [FromQuery] int end_year,
    [FromServices] ApplicationDbContext dbContext) =>
{
    var experience = await dbContext.Experiences
        .FirstOrDefaultAsync(e => e.experience_id == id);

    if (experience == null)
    {
        return Results.NotFound("Experience not found");
    }

    experience.company_name = company_name;
    experience.start_year = start_year;
    experience.end_year = end_year;

    await dbContext.SaveChangesAsync();

    return Results.Ok(new
    {
        experience.experience_id,
        experience.company_name,
        experience.start_year,
        experience.end_year,
        profile_id = experience.fk_profile_id
    });
});


app.MapDelete("/experiences/{id}", async (int id, ApplicationDbContext dbContext) =>
{
    var experience = await dbContext.Experiences
        .FirstOrDefaultAsync(e => e.experience_id == id);

    if (experience == null)
    {
        return Results.NotFound("Experience not found");
    }

    dbContext.Experiences.Remove(experience);
    await dbContext.SaveChangesAsync();

    return Results.NoContent(); // 204 No Content
});



//TALENT PROFILES 
app.MapGet("/talent_profiles/list", async (ApplicationDbContext db) =>
{
    var publicProfiles = await db.TalentProfiles
        .Where(p => p.privacy == 0)
        .Include(p => p.TalentProfileSkills)
        .ThenInclude(s => s.Skill)
        .Include(p => p.Experiences)
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
            }).ToList(),
            Experiences = p.Experiences.Select(e => new ExperienceDto
            {
                ExperienceId = e.experience_id,
                CompanyName = e.company_name,
                StartYear = e.start_year,
                EndYear = e.end_year
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
        .Include(p => p.Experiences)
        .FirstOrDefaultAsync(p => p.profile_id == id);

    if (profile == null)
        return Results.NotFound();

    if (profile.privacy != 0)
        return Results.Unauthorized();

    var profileDto = new TalentProfileDto
    {
        ProfileId = profile.profile_id,
        ProfileName = profile.profile_name,
        Country = profile.country,
        Email = profile.email,
        Price = profile.price,
        Privacy = profile.privacy,
        Category = profile.category,
        FkUserId = profile.fk_user_id,
        Skills = profile.TalentProfileSkills.Select(s => new SkillDto
        {
            SkillId = s.SkillId,
            SkillName = s.Skill.name,
            YearsOfExperience = s.YearsOfExperience
        }).ToList(),
        Experiences = profile.Experiences.Select(e => new ExperienceDto
        {
            ExperienceId = e.experience_id,
            CompanyName = e.company_name,
            StartYear = e.start_year,
            EndYear = e.end_year
        }).ToList()
    };

    return Results.Ok(profileDto);
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




app.UseHttpsRedirection();

app.Run();