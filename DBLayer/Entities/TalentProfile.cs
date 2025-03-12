using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ESOF.WebApp.DBLayer.Entities;

public class TalentProfile
{
    [Key]
    public Guid ProfileID { get; set; }
    
    public string Name { get; set; }
    public string Country { get; set; }
    public string Email { get; set; }
    public float Price { get; set; }
    public string Privacy { get; set; }

    public Guid UserID { get; set; }
    [ForeignKey("UserID")]
    public User User { get; set; }
   
    
    public ICollection<Experience> Experience { get; set; }
    public ICollection<ProfileSkill> ProfileSkill { get; set; }

}
