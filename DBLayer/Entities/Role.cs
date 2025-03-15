using System.ComponentModel.DataAnnotations;

namespace ESOF.WebApp.DBLayer.Entities;

public class Role
{
    [Key]
    public int role_id { get; set; }
    public string role { get; set; }
    public int role_level { get; set; }
    
    //one-to-many
    public ICollection<User> Users { get; set; }
}