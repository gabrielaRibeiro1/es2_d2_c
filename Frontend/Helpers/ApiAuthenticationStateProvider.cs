namespace Frontend.Helpers;

using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

public class ApiAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly HttpClient _httpClient;

    public ApiAuthenticationStateProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity()); // Usuário não autenticado por padrão

        try
        {
            var response = await _httpClient.GetAsync("api/auth/me"); // Criamos um endpoint `me`
            if (response.IsSuccessStatusCode)
            {
                var userInfo = await response.Content.ReadFromJsonAsync<UserInfo>();

                var identity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, userInfo.Username),
                    new Claim(ClaimTypes.Role, userInfo.RoleId.ToString())
                }, "apiauth");

                user = new ClaimsPrincipal(identity);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao buscar autenticação: {ex.Message}");
        }

        return new AuthenticationState(user);
    }

    public void NotifyUserAuthentication(string username)
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, username)
        }, "apiauth");

        var user = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public void NotifyUserLogout()
    {
        var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymousUser)));
    }

    private class UserInfo
    {
        public string Username { get; set; }
        public int RoleId { get; set; }
    }
}
