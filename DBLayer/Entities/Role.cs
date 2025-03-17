using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ESOF.WebApp.DBLayer.Entities;

public class Role
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int role_id { get; set; }
    public string role { get; set; }
    
    //one-to-many
    public ICollection<User> Users { get; set; }
}