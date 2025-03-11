using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ESOF.WebApp.DBLayer.Entities;

public class Experience
{
    [Key]
    public Guid ExperienceID { get; set; }
    
    
    public string Title { get; set; }

    public string CompanyName { get; set; }

    public int StartYear { get; set; }

    public int EndYear { get; set; }

    public Guid ProfileID  { get; set; }

    [ForeignKey("ProfileID")]
    public TalentProfile TalentProfile { get; set; }
    

}
