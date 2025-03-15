using System.ComponentModel.DataAnnotations;

namespace ESOF.WebApp.DBLayer.Entities;

public class WorkProposal
{
    [Key]
    public int proposal_id { get; set; }
    public string proposal_name { get; set; }
    public string category { get; set; }
    public string necessary_skills { get; set; }
    public string years_of_experience { get; set; }
    public string description { get; set; }
    public string total_hours { get; set; }
    
    //many-to-one
    public int fk_user_id { get; set; }
    public User User { get; set; }
}