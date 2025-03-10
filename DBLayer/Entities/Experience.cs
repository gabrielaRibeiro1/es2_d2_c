using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ESOF.WebApp.DBLayer.Entities;

public class Experience
{
    [Key]
    public Guid ExperienceID { get; set; }
    

}
