using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ESOF.WebApp.DBLayer.Entities;

public class Skill
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int skill_id { get; set; }
    public string name { get; set; }
    public string area { get; set; }
    //one-to-many
    public ICollection<UserSkill> UserSkills { get; set; }
}