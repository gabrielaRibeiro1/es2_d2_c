namespace Frontend.Helpers;

public static class UserSession
{
    public static int? UserId { get; set; }
    public static string? Username { get; set; }
    public static int? RoleId { get; set; }

    public static void Clear()
    {
        UserId = null;
        Username = null;
        RoleId = null;
    }
}