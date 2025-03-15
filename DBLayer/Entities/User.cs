using System.ComponentModel.DataAnnotations;

namespace ESOF.WebApp.DBLayer.Entities;

public class User
{
    [Key]
    public int user_id { get; set; }
    public string username { get; set; }
    public string password { get; set; }
    
    //many-to-one
    public int fk_role_id { get; set; }
    public Role Role { get; set; }
    
    //one-to-many
    public ICollection<WorkProposal> Proposals { get; set; }
    public ICollection<TalentProfile> Profiles { get; set; }
    public ICollection<UserSkill> UserSkills { get; set; }
    
}