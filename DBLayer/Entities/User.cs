using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ESOF.WebApp.DBLayer.Entities;

public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int user_id { get; set; }
    public string username { get; set; }
    public byte[] passwordHash { get; set; }
    public byte[] passwordSalt { get; set; }
    
    //many-to-one
    public int fk_role_id { get; set; }
    public Role Role { get; set; }
    
    //one-to-many
    public ICollection<WorkProposal> Proposals { get; set; }
    public ICollection<TalentProfile> Profiles { get; set; }
    public ICollection<UserSkill> UserSkills { get; set; }
    
       
}