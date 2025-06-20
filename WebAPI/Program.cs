using ESOF.WebApp.DBLayer.Context;
using ESOF.WebApp.DBLayer.Entities;
using ESOF.WebApp.DBLayer.Helpers;
using ESOF.WebApp.Services.Reports;
using ESOF.WebApp.WebAPI.DTOs;
using ESOF.WebApp.WebAPI.Models;
using Helpers.Models;
using Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    var exists = await db.Users.AnyAsync(u => u.username == request.Username);
    if (exists)
    {
        return Results.Conflict($"The username '{request.Username}' is already in use.");
    }
    
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

app.MapPost("/login", async (LoginRequest login, ApplicationDbContext db) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.username == login.Username);
    if (user == null)
        return Results.Unauthorized();

    if (!PasswordHelper.VerifyPassword(login.Password, user.passwordHash, user.passwordSalt))
        return Results.Unauthorized();

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
    var result = await db.Users
        .Where(u => u.username == username)
        .Join(db.Roles,
            u => u.fk_role_id,
            r => r.role_id,
            (u, r) => new UserViewModel
            {
                User_id    = u.user_id,
                Username   = u.username,
                fk_role_id = u.fk_role_id,
                RoleName   = r.role
            })
        .FirstOrDefaultAsync();

    if (result == null)
        return Results.NotFound("User not found.");

    return Results.Ok(result);
});




app.MapGet("/get_all_users", async (ApplicationDbContext db) =>
{
    var users = await db.Users
        .Join(db.Roles, 
            u => u.fk_role_id,   
            r => r.role_id,           
            (u, r) => new        
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
    var user = await db.Users.FindAsync(id);
    if (user == null)
    {
        return Results.NotFound("User not found.");
    }
    
    db.Users.Remove(user);
    await db.SaveChangesAsync();

    return Results.Ok($"User with ID {id} deleted successfully.");
});

app.MapPut("/update_user/{id:int}", async (int id, string? newPassword, int? newRoleId, ApplicationDbContext db) =>
{
    
    var user = await db.Users.FirstOrDefaultAsync(u => u.user_id == id);
    if (user == null)
    {
        return Results.NotFound("User not found.");
    }
    
    if (!string.IsNullOrEmpty(newPassword))
    {
        PasswordHelper.CreatePasswordHash(newPassword, out byte[] passwordHash, out byte[] passwordSalt);
        user.passwordHash = passwordHash;
        user.passwordSalt = passwordSalt;
    }
    
    if (newRoleId.HasValue)
    {
        user.fk_role_id = newRoleId.Value;
    }

    await db.SaveChangesAsync();
    return Results.Ok("User updated successfully.");
});

app.MapGet("/get_user_by_id/{id:int}", async (int id, ApplicationDbContext db) =>
{
    var user = await db.Users
        .Include(u => u.Role)
        .FirstOrDefaultAsync(u => u.user_id == id);

    if (user == null)
        return Results.NotFound(new { message = "User not found." });

    return Results.Ok(new
    {
        user.user_id,
        user.username,
        roleId = user.fk_role_id,
        roleName = user.Role?.role
    });
});

app.MapPost("/add_user", async (UserAddModel model, ApplicationDbContext db) =>
    {
        // 1. Verifica se o username já existe
        var exists = await db.Users.AnyAsync(u => u.username == model.Username);
        if (exists)
        {
            return Results.Conflict($"The username '{model.Username}' is already in use.");
        }
        
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
        return Results.NotFound("User not found.");
    }
    
    if (!string.IsNullOrEmpty(updateData.Password))
    {
        PasswordHelper.CreatePasswordHash(updateData.Password, out byte[] passwordHash, out byte[] passwordSalt);
        user.passwordHash = passwordHash;
        user.passwordSalt = passwordSalt;
    }

    if (updateData.RoleId >= 1 && updateData.RoleId <= 3 && user.fk_role_id != updateData.RoleId)
    {
        user.fk_role_id = updateData.RoleId;
    }
    
    await db.SaveChangesAsync();
    return Results.Ok("User updated successfully.");
});

// Endpoint to create a skill
app.MapPost("/skills", async ([FromBody] Skill skill, ApplicationDbContext db) =>
{
    db.Skills.Add(skill);
    await db.SaveChangesAsync();

    return Results.Ok($"Skill '{skill.name}' successfully created!");
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
        return Results.NotFound("Skill not found!");
    
    skill.name = updatedSkill.name;
    skill.area = updatedSkill.area;

    await db.SaveChangesAsync();
    return Results.Ok($"Skill {id} updated successfully!");
});


app.MapDelete("/skills/{id}", async (int id, ApplicationDbContext db) =>
{
    var skill = await db.Skills
        .Include(s => s.UserSkills)
        .FirstOrDefaultAsync(s => s.skill_id == id);

    if (skill == null)
        return Results.NotFound("Skill not found.");
    
    var isUsedInTalentProfiles = await db.TalentProfileSkills.AnyAsync(tps => tps.SkillId == id);

    if (isUsedInTalentProfiles)
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

app.MapGet("/work_proposals/{proposalId}/eligible_talents", 
    async (int proposalId, ApplicationDbContext db) => 
{
    var proposal = await db.WorkProposals
        .AsNoTracking()
        .FirstOrDefaultAsync(p => p.proposal_id == proposalId);

    if (proposal == null)
        return Results.NotFound("Work proposal not found.");
    
    var eligibleTalents = await db.TalentProfiles
        .Where(p => p.category == proposal.category 
                 && p.privacy == 0)
        .Include(p => p.TalentProfileSkills)
            .ThenInclude(tps => tps.Skill)
        .Include(p => p.Experiences)
        .AsNoTracking()
        .ToListAsync();
    
    var talentDtos = eligibleTalents
        .Select(p => {
            var totalValue = p.price * proposal.total_hours;

            return new TalentProfileDto
            {
                ProfileId   = p.profile_id,
                ProfileName = p.profile_name,
                Country     = p.country,
                Email       = p.email,
                Price       = p.price,
                Privacy     = p.privacy,
                Category    = p.category,
                FkUserId    = p.fk_user_id,
                Skills      = p.TalentProfileSkills
                                  .Select(s => new SkillDto {
                                      SkillId           = s.SkillId,
                                      SkillName         = s.Skill.name,
                                      YearsOfExperience = s.YearsOfExperience
                                  })
                                  .ToList(),
                Experiences = p.Experiences
                                  .Select(e => new ExperienceDto {
                                      ExperienceId = e.experience_id,
                                      CompanyName  = e.company_name,
                                      StartYear    = e.start_year,
                                      EndYear      = e.end_year
                                  })
                                  .ToList(),
                TotalValue  = totalValue
            };
        })
        .OrderBy(x => x.TotalValue)
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
    .WithSummary("Gets the average monthly price by category and country.")
    .WithDescription("Based on 176 hours per month, it groups talent by category and country.\n")
    .Produces<Dictionary<string, float>>(StatusCodes.Status200OK);

app.MapGet("/reports/skill", async (ApplicationDbContext db) =>
    {
        var report = new SkillReport(db).GenerateReport();
        return Results.Ok(report);
    })
    .WithName("GetSkillReport")
    .WithSummary("Gets the average monthly price by skill.")
    .WithDescription("Based on 176 hours per month, it groups talent by skills.")
    .Produces<Dictionary<string, float>>(StatusCodes.Status200OK);


//EXPERIENCES
app.MapPost("/talent_profiles/{profile_name}/add_experience", 
    async (string profile_name, 
        [FromQuery] string company_name, 
        [FromQuery] int start_year, 
        [FromQuery] int? end_year, 
        ApplicationDbContext db) =>
    {
        if (end_year.HasValue && start_year > end_year.Value)
            return Results.BadRequest("The starting year cannot be later than the ending year.");
        
        var profile = await db.TalentProfiles
            .Include(p => p.Experiences)
            .FirstOrDefaultAsync(p => p.profile_name == profile_name);

        if (profile == null)
            return Results.NotFound("Talent profile not found.");
        
        bool overlaps = profile.Experiences.Any(e =>
        {
            int eEnd = (e.end_year == 0) ? int.MaxValue : e.end_year;
            int newEnd = end_year ?? int.MaxValue;
            return start_year <= eEnd && newEnd >= e.start_year;
        });

        if (overlaps)
            return Results.BadRequest("The new experience overlaps with an existing one.");
        
        var newExperience = new Experience
        {
            company_name = company_name,
            start_year = start_year,
            end_year = end_year ?? 0, 
            fk_profile_id = profile.profile_id
        };
        profile.Experiences.Add(newExperience);
        await db.SaveChangesAsync();
        
        return Results.Ok("Experience added successfully.");
    });


app.MapGet("/experiences/by-profile", async (
    [FromQuery] string profileName,
    [FromServices] ApplicationDbContext dbContext) =>
{
    var profile = await dbContext.TalentProfiles
        .FirstOrDefaultAsync(p => p.profile_name == profileName);

    if (profile == null)
    {
        return Results.NotFound("Profile not found");
    }
    
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


app.MapGet("/experiences", async (ApplicationDbContext db) =>
{
    var list = await db.Experiences
        .Include(e => e.Profile)
        .Select(e => new ExperienceDto()
        {
            ExperienceId = e.experience_id,
            CompanyName  = e.company_name,
            StartYear    = e.start_year,
            EndYear      = e.end_year,
            ProfileName  = e.Profile.profile_name
        })
        .ToListAsync();

    return Results.Ok(list);
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

    return Results.NoContent(); 
});



//TALENT PROFILES 
app.MapGet("/talent_profiles/list", async (ApplicationDbContext db) =>
{
    var allProfiles = await db.TalentProfiles
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

    return Results.Ok(allProfiles);
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
    [FromQuery] string skill_names,
    [FromServices] ApplicationDbContext db) =>
{
    if (string.IsNullOrEmpty(skill_names))
        return Results.BadRequest("At least one skill name must be provided.");

    var skillList = skill_names
        .Split(',', StringSplitOptions.RemoveEmptyEntries)
        .Select(s => s.Trim().ToLower())
        .ToList();

    // bring in skills + experiences
    var profiles = await db.TalentProfiles
        .Where(p => p.privacy == 0)
        .Include(p => p.TalentProfileSkills)
            .ThenInclude(tps => tps.Skill)
        .Include(p => p.Experiences)
        .ToListAsync();

    var filtered = profiles
        .Where(p => skillList
            .All(sn => p.TalentProfileSkills
                .Any(tps => tps.Skill.name.ToLower() == sn)))
        .OrderBy(p => p.profile_name)
        .Select(p => new TalentProfileDto
        {
            ProfileId   = p.profile_id,
            ProfileName = p.profile_name,
            Country     = p.country,
            Email       = p.email,
            Price       = p.price,
            Privacy     = p.privacy,
            Category    = p.category,
            FkUserId    = p.fk_user_id,
            Skills      = p.TalentProfileSkills
                            .Select(s => new SkillDto {
                                SkillId           = s.SkillId,
                                SkillName         = s.Skill.name,
                                YearsOfExperience = s.YearsOfExperience
                            }).ToList(),
            Experiences = p.Experiences
                            .Select(e => new ExperienceDto {
                                ExperienceId = e.experience_id,
                                CompanyName  = e.company_name,
                                StartYear    = e.start_year,
                                EndYear      = e.end_year,
                                ProfileName  = p.profile_name  
                            }).ToList()
        })
        .ToList();

    return Results.Ok(filtered);
});


app.MapGet("/skills/list", async (ApplicationDbContext db) =>
{
    var skills = await db.Skills
        .Select(s => s.name)
        .ToListAsync();

    return Results.Ok(skills);
});

//Add user with role 1 automatically
app.MapPost("/create_account2", async ([FromBody] CreateUserDto dto, ApplicationDbContext db) =>
{
    var exists = await db.Users.AnyAsync(u => u.username == dto.Username);
    if (exists)
    {
        return Results.Conflict($"O username '{dto.Username}' já está em uso.");
    }
    if (string.IsNullOrEmpty(dto.Password))
        return Results.BadRequest("Password cannot be empty.");

    PasswordHelper.CreatePasswordHash(dto.Password, out byte[] passwordHash, out byte[] passwordSalt);

    if (passwordHash == null || passwordSalt == null)
        return Results.BadRequest("Error generating password hash and salt.");

    var newUser = new User
    {
        username     = dto.Username,
        passwordHash = passwordHash,
        passwordSalt = passwordSalt,
        fk_role_id   = 1
    };

    db.Users.Add(newUser);
    await db.SaveChangesAsync();

    return Results.Created($"/users/{newUser.user_id}", newUser);
});



app.UseHttpsRedirection();

app.Run();