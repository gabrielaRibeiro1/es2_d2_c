namespace Frontend.Models;

public class UserUpdateModel
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty; // Empty if unchanged
    public int RoleId { get; set; }
}