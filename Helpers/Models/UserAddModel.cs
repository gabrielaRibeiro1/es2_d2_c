using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Helpers.Models
{
    public class UserAddModel
    {
        
        public int? UserId { get; set; } 

        [Required(ErrorMessage = "O campo username é obrigatório.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "O campo senha é obrigatório.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "O campo role é obrigatório.")]
        [JsonPropertyName("fk_role_id")]
        public int RoleId { get; set; }
    }
}