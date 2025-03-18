using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ESOF.WebApp.DBLayer.Entities;

public class Experience
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int experience_id { get; set; }
    public string company_name { get; set; }
    public string start_year { get; set; }
    public string end_year { get; set; }

    //many-to-one
    public int fk_profile_id { get; set; }
    public TalentProfile Profile { get; set; }
}