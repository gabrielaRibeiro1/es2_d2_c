using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ESOF.WebApp.DBLayer.Entities;

public class Skills
{
    [Key]
    public Guid SkillID { get; set; }
    
    public string Name { get; set; }

    public string Area { get; set; }

    public Guid WorkSkillID  { get; set; }
    public Guid ProfileSkillID  { get; set; }
    
    [ForeignKey("WorkSkillID")]
    public WorkSkill WorkSkill { get; set; }
    
    [ForeignKey("ProfileSkillID")]
    public ProfileSkill ProfileSkill { get; set; }
}
