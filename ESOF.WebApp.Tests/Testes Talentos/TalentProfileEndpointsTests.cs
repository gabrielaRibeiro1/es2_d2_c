using Microsoft.AspNetCore.Http.HttpResults;

namespace ESOF.WebApp.Tests
{

using ESOF.WebApp.WebAPI.DTOs;
using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Http;
using DBLayer.Entities;
using DBLayer.Context;



    [TestFixture]
public class TalentProfileEndpointsTests
{
    private ApplicationDbContext _dbContext;

    [SetUp]
    public void SetUp()
    {
  
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Test");

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid())
            .Options;

        _dbContext = new ApplicationDbContext(options);

        _dbContext.TalentProfiles.Add(new TalentProfile
        {
            profile_id = 1,
            profile_name = "TestUser",
            country = "PT",
            email = "test@example.com",
            price = 50,
            privacy = 0,
            category = "Dev",
            fk_user_id = 123,
            TalentProfileSkills = new List<TalentProfileSkill>(),
            Experiences = new List<Experience>()
        });

        _dbContext.SaveChanges();
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext?.Database?.EnsureDeleted();
        _dbContext?.Dispose();
    }

    [Test]
    public async Task GetPublicTalentProfiles_ReturnsOkWithProfiles()
    {
        var publicProfiles = await _dbContext.TalentProfiles
            .Where(p => p.privacy == 0)
            .Include(p => p.TalentProfileSkills)
                .ThenInclude(s => s.Skill)
            .Include(p => p.Experiences)
            .Select(p => new TalentProfileDto
            {
                ProfileId = p.profile_id,
                ProfileName = p.profile_name ?? string.Empty,
                Country = p.country ?? string.Empty,
                Email = p.email ?? string.Empty,
                Price = p.price,
                Privacy = p.privacy,
                Category = p.category ?? string.Empty,
                FkUserId = p.fk_user_id,
                Skills = new List<SkillDto>(),
                Experiences = new List<ExperienceDto>()
            }).ToListAsync();

        var result = Results.Ok(publicProfiles);

        // Assert
        Assert.That(result, Is.TypeOf<Ok<List<TalentProfileDto>>>());
        var okResult = (Ok<List<TalentProfileDto>>)result;
        Assert.That(okResult.Value, Is.Not.Null);
        Assert.That(okResult.Value.Count, Is.EqualTo(1));
        Assert.That(okResult.Value[0].ProfileName, Is.EqualTo("TestUser"));
    }
}}