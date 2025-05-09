using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Frontend.Components;
using Frontend.Helpers;
using Helpers;

var builder = WebApplication.CreateBuilder(args);

// Registre serviços de páginas Razor e Blazor Server
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Configure serviços de autenticação e autorização
builder.Services.AddOptions();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/access-denied";
    });
builder.Services.AddAuthorizationCore();

// Registre o AuthenticationStateProvider customizado
builder.Services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();

// Antiforgery (para proteger formulários e requisições)
builder.Services.AddAntiforgery();

// Registre HttpClient para comunicação com a API usando BaseAddress do EnvFileHelper
builder.Services.AddHttpClient<ApiHelper>(client =>
{
    client.BaseAddress = new Uri(EnvFileHelper.GetString("API_URL"));
})
.ConfigurePrimaryHttpMessageHandler(() =>
    new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });

// Registre HttpClient específico para o ApiAuthenticationStateProvider
builder.Services.AddHttpClient<ApiAuthenticationStateProvider>(client =>
{
    client.BaseAddress = new Uri(EnvFileHelper.GetString("API_URL"));
})
.ConfigurePrimaryHttpMessageHandler(() =>
    new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });

// Registre componentes interativos Razor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Middleware de autenticação e autorização
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// Mapeie componentes
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
