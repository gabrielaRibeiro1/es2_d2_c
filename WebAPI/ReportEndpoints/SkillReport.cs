using ESOF.WebApp.DBLayer;
using ESOF.WebApp.DBLayer.Context;
using ESOF.WebApp.DBLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace ESOF.WebApp.Services.Reports;

public class SkillReport : ReportGenerator
{
    private readonly ApplicationDbContext _context;

    public SkillReport(ApplicationDbContext context)
    {
        _context = context;
    }

    protected override List<TalentProfile> GetData()
    {
        return _context.TalentProfiles
            .Include(tp => tp.User)
            .ThenInclude(u => u.UserSkills)
            .ThenInclude(us => us.Skill)
            .ToList();
    }

    protected override Dictionary<string, List<float>> GroupData(List<TalentProfile> data)
    {
        var result = new Dictionary<string, List<float>>();

        foreach (var profile in data)
        {
            foreach (var userSkill in profile.User.UserSkills)
            {
                var skillName = userSkill.Skill.name;

                if (!result.ContainsKey(skillName))
                    result[skillName] = new List<float>();

                result[skillName].Add(profile.price);
            }
        }

        return result;
    }
}