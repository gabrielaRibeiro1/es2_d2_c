using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using ESOF.WebApp.WebAPI.DTOs;
using BLL.Services;
using ESOF.WebApp.DBLayer.Context;
using ESOF.WebApp.DBLayer.Entities;

namespace ESOF.WebApp.Tests
{
    [TestFixture]
    public class TalentProfileServiceTests
    {
        private ApplicationDbContext _dbContext;
        private TalentProfileService _service;

        [SetUp]
        public void SetUp()
        {
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

            _service = new TalentProfileService(_dbContext);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext?.Database.EnsureDeleted();
            _dbContext?.Dispose();
        }

        [Test]
        public async Task GetPublicProfilesAsync_ReturnsPublicProfiles()
        {
            var result = await _service.GetPublicProfilesAsync();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].ProfileName, Is.EqualTo("TestUser"));
        }
    }
}