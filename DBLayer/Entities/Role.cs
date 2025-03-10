using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ESOF.WebApp.DBLayer.Entities;

public class Role
{
    [Key]
    public Guid RoleID { get; set; }
    
    public string Name { get; set; }

    public ICollection<User> Users { get; set; }
}
