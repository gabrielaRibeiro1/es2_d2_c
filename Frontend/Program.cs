using Microsoft.AspNetCore.Components.Authorization;
using Frontend.Components;
using Frontend.Helpers;
using Helpers;

var builder = WebApplication.CreateBuilder(args);

// Adicionar serviços de autenticação no Blazor
builder.Services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();
builder.Services.AddAuthorizationCore();
builder.Services.AddAntiforgery();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(EnvFileHelper.GetString("API_URL")) });
builder.Services.AddScoped<ApiHelper>();

// Adiciona os serviços necessários para Razor Components e autorização
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAuthorization();  // <-- Adicione esta linha

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthorization(); // <-- Adicione esta linha

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
