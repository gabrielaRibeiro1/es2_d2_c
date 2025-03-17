using System.ComponentModel.DataAnnotations;

namespace ESOF.WebApp.DBLayer.Entities;

public class UserSkill
{
    [Key]
    public int UserId { get; set; }
    public User User { get; set; }

    [Key]
    public int SkillId { get; set; }
    public Skill Skill { get; set; }
}
