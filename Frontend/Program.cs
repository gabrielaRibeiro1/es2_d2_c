using Microsoft.AspNetCore.Components.Authorization;
using Frontend.Components;
using Frontend.Helpers;
using Helpers;

var builder = WebApplication.CreateBuilder(args);

// Registre serviços de páginas Razor e Blazor Server
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Autorização baseada em Claims (obrigatório para <AuthorizeView>)
builder.Services.AddAuthorizationCore();

// Registar o AuthenticationStateProvider personalizado
builder.Services.AddScoped<ApiAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<ApiAuthenticationStateProvider>());

// HttpClient configurado para todos os serviços
builder.Services.AddHttpClient("API", client =>
    {
        client.BaseAddress = new Uri(EnvFileHelper.GetString("API_URL"));
    })
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });

// Regista o HttpClient padrão usando o nome "API"
builder.Services.AddScoped(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return factory.CreateClient("API");
});

// Antiforgery
builder.Services.AddAntiforgery();

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
