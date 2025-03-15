using System.ComponentModel.DataAnnotations;

namespace ESOF.WebApp.DBLayer.Entities;

public class Skill
{
    [Key]
    public int skill_id { get; set; }
    public string name { get; set; }
    public string area { get; set; }
    //one-to-many
    public ICollection<UserSkill> UserSkills { get; set; }
}