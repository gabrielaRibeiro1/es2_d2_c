namespace Helpers.Models
{
    public class UserViewModel
    {
        public int User_id { get; set; }
        public string Username { get; set; }
        public int fk_role_id { get; set; }
        
        public string RoleName { get; set; }
    }
}