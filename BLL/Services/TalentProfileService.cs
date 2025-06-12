using ESOF.WebApp.DBLayer.Context;
using ESOF.WebApp.DBLayer.Entities;
using ESOF.WebApp.WebAPI.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services
{
    public class TalentProfileService
    {
        private readonly ApplicationDbContext _context;

        public TalentProfileService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<TalentProfileDto>> GetPublicProfilesAsync()
        {
            return await _context.TalentProfiles
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
                })
                .ToListAsync();
        }

        public async Task<TalentProfile> CreateTalentProfileAsync(TalentProfile profile)
        {
            _context.TalentProfiles.Add(profile);
            await _context.SaveChangesAsync();
            return profile;
        }

        public async Task<TalentProfile> UpdateTalentProfileAsync(TalentProfile profile)
        {
            var existing = await _context.TalentProfiles.FindAsync(profile.profile_id);
            if (existing == null)
                throw new InvalidOperationException("Profile not found");

            existing.profile_name = profile.profile_name;
            existing.country = profile.country;
            existing.email = profile.email;
            existing.price = profile.price;
            existing.privacy = profile.privacy;
            existing.category = profile.category;
            existing.fk_user_id = profile.fk_user_id;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteTalentProfileAsync(int id)
        {
            var profile = await _context.TalentProfiles.FindAsync(id);
            if (profile == null)
                return false;

            _context.TalentProfiles.Remove(profile);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
