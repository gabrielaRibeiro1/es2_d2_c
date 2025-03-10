using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ESOF.WebApp.DBLayer.Entities;

public class ProfileSkill
{
    [Key]
    public Guid ProfileSkillID { get; set; }
    
    public Guid ProfileID  { get; set; }
    [ForeignKey("ProfileID")]
    public TalentProfile TalentProfile { get; set; }
    
    public Guid SkillID  { get; set; }
    [ForeignKey("SkillID")]
    public Skills Skills { get; set; }
    
   
}
