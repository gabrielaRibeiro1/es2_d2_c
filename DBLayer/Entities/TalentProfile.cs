using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ESOF.WebApp.DBLayer.Entities;

public class TalentProfile
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int profile_id { get; set; }
    public string profile_name { get; set; }
    public string country { get; set; }
    public string email { get; set; }
    public float price { get; set; }
    public int privacy { get; set; }
    public string category { get; set; }
    
    //many-to-one
    public int fk_user_id { get; set; }
    public User User { get; set; }
    
    //one-to-many
    public ICollection<Experience> Experiences { get; set; }
    public ICollection<TalentProfileSkill> TalentProfileSkills { get; set; }

}