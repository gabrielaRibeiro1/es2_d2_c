using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ESOF.WebApp.DBLayer.Entities;

public class WorkSkill
{
    
    public Guid ProposalID  { get; set; }
    [ForeignKey("ProposalID")]
    public WorkProposals WorkProposals { get; set; }
    
    public Guid SkillID  { get; set; }
    [ForeignKey("SkillID")]
    public Skills Skills { get; set; }
}
