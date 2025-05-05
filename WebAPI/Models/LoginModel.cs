namespace ESOF.WebApp.WebAPI.Models;

public class LoginModel
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty; // Empty if unchanged
}
