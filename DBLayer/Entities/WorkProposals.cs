using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ESOF.WebApp.DBLayer.Entities;

public class WorkProposals
{
    [Key]
    public Guid ProposalID { get; set; }
    
    public string Name { get; set; }

    public string Category { get; set; }

    public int ExperienceYears { get; set; }

    public string Description { get; set; }
    
    public float TotalHours { get; set; }

    public Guid UserID  { get; set; }
    public Guid WorkSkillID  { get; set; }
    
    [ForeignKey("UserID")]
    public User User { get; set; }
    
    [ForeignKey("WorkSkillID")]
    public WorkSkill WorkSkill { get; set; }
}

