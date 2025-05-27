namespace ESOF.WebApp.WebAPI.Models;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public int Role;
}