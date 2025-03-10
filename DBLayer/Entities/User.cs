using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ESOF.WebApp.DBLayer.Entities;

public class User
{
    [Key]
    public Guid UserID { get; set; }
    
    public string Username { get; set; }
    
    public string Password { get; set; }
    
    public Guid RoleID  { get; set; }
    
    [ForeignKey("RoleID")]
    public Role Role { get; set; }

    public ICollection<TalentProfile> TalentProfile { get; set; }
    public ICollection<WorkProposals> WorkProposals { get; set; }
}
