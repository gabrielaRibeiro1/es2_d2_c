using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace Frontend.Helpers;

public class ApiAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly HttpClient _httpClient;
    private readonly IJSRuntime   _jsRuntime;

    public ApiAuthenticationStateProvider(HttpClient httpClient, IJSRuntime jsRuntime)
    {
        _httpClient = httpClient;
        _jsRuntime  = jsRuntime;
    }
    
    public async Task<bool> LoginAsync(LoginModel login)
    {
        var response = await _httpClient.PostAsJsonAsync("login", login);
        if (!response.IsSuccessStatusCode)
            return false;

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

        // 1) Store token (only in interactive mode)
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", loginResponse.Token);
        Console.WriteLine("Escrevi raw localStorage: " + loginResponse.Token);

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginResponse.Token);

        MarkUserAsAuthenticated(loginResponse.Token);

        return true;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        string token = null;
        try
        {
            token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
            Console.WriteLine("Lido raw localStorage: " + token);
        }
        catch (InvalidOperationException)
        {
            // prerendering: no token available yet
        }
        
        if (string.IsNullOrEmpty(token))
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        // Set HttpClient header
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        // Rebuild user
        var handler = new JwtSecurityTokenHandler();
        var jwt     = handler.ReadJwtToken(token);
        var identity = new ClaimsIdentity(jwt.Claims, "jwt");
        var user     = new ClaimsPrincipal(identity);
        
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        return new AuthenticationState(user);
    }
    
    public void MarkUserAsAuthenticated(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        var identity = new ClaimsIdentity(jwt.Claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }
    
    public void RefreshAuthenticationState() 
        => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    
    public class LoginModel {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
    
    public record LoginResponse(string Token, string Username, int Role);
}
