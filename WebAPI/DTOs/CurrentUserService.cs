namespace ESOF.WebApp.WebAPI.DTOs;

public class CurrentUserService
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public int RoleId { get; set; }

    public bool IsLoggedIn => UserId > 0;

    public void Clear()
    {
        UserId = 0;
        Username = string.Empty;
        RoleId = 0;
    }
}
