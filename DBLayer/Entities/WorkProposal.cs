using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ESOF.WebApp.DBLayer.Entities;

public class WorkProposal
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    
    public int proposal_id { get; set; }
    public string proposal_name { get; set; }
    public string category { get; set; }
    public string necessary_skills { get; set; }
    public int years_of_experience { get; set; }
    public string description { get; set; }
    public int total_hours { get; set; }
    
    //many-to-one
    public int fk_user_id { get; set; }
    public User User { get; set; }
}