using System.ComponentModel.DataAnnotations;

namespace ESOF.WebApp.DBLayer.Entities;

public class TalentProfileSkill
{
    [Key]
    public int Id { get; set; }

    public int TalentProfileId { get; set; }
    public TalentProfile TalentProfile { get; set; }

    public int SkillId { get; set; }
    public Skill Skill { get; set; }

    public int YearsOfExperience { get; set; }
}
