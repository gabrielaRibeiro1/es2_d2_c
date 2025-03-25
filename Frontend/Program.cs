using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies; 
using Frontend.Components;
using Frontend.Helpers;
using Helpers;


var builder = WebApplication.CreateBuilder(args);

// Adicionar serviços de autenticação no Blazor
// Adiciona autenticação
builder.Services.AddOptions();

// 🔥 1️⃣ Registra o serviço de autenticação
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login"; // Defina a rota de login
        options.AccessDeniedPath = "/access-denied"; // Rota de acesso negado (se necessário)
    });

builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();
builder.Services.AddAntiforgery();
//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(EnvFileHelper.GetString("API_URL")) });
builder.Services.AddHttpClient<ApiAuthenticationStateProvider>(client =>
    {
        client.BaseAddress = new Uri(EnvFileHelper.GetString("API_URL"));
    })
    .ConfigurePrimaryHttpMessageHandler(() =>
        new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });
builder.Services.AddScoped<ApiHelper>();

// Adiciona os serviços necessários para Razor Components e autorização
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
