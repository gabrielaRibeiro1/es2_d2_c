namespace Frontend.Helpers;

public class LoginEventService
{
    public event Action? OnLoginStatusChanged;

    public void NotifyLoginStatusChanged() => OnLoginStatusChanged?.Invoke();
}