using System.Text;
using Frontend.Components.Pages;

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
        var user = new ClaimsPrincipal(new ClaimsIdentity()); 

        try
        {
            var dados = new {username = "ana", password= "1234"};
            var json = JsonSerializer.Serialize(dados);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://localhost:7103/login", content);
        //   var response = await _httpClient.GetAsync("https://localhost:7103/login");
        

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
            Console.WriteLine($"{response}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao buscar autenticação: {ex.Message}");
        }

        return new AuthenticationState(user);
    }
    
    public async Task<bool> LoginAsync(LoginModel login)
    {
        var response = await _httpClient.PostAsJsonAsync("login", login);
        if (response.IsSuccessStatusCode)
        {
            // Se a API retornar informações do usuário, você pode usá-las aqui
            var userInfo = await response.Content.ReadFromJsonAsync<UserInfo>();
            NotifyUserAuthentication(userInfo.Username);
            return true;
        }
        return false;
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
    
    
    public class LoginModel {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
