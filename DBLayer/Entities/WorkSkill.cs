using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ESOF.WebApp.DBLayer.Entities;

public class WorkSkill
{
    [Key]
    public Guid WorkSkillID { get; set; }

    
    public Guid ProposalID  { get; set; }
    [ForeignKey("ProposalID")]
    public WorkProposals WorkProposals { get; set; }
    
    public Guid SkillID  { get; set; }
    [ForeignKey("SkillID")]
    public Skills Skills { get; set; }
}
