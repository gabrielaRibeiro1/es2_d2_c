using Frontend.Components;
using Frontend.Helpers;
using Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Adicionar serviços ao contêiner.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configuração do HttpClient com IHttpClientFactory para facilitar o gerenciamento do ciclo de vida
builder.Services.AddHttpClient<ApiHelper>(client =>
{
    var apiUrl = builder.Configuration["API_URL"]; // Certifique-se de que a chave API_URL está no arquivo de configuração ou variável de ambiente
    if (string.IsNullOrEmpty(apiUrl))
    {
        throw new InvalidOperationException("API_URL não está configurada.");
    }
    client.BaseAddress = new Uri(apiUrl);
});

builder.Services.AddScoped<ApiHelper>();

var app = builder.Build();

// Configuração do pipeline de requisição HTTP.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true); // Lidar com exceções em produção
    app.UseHsts(); // Segurança: Ativar HSTS para ambientes de produção
}

app.UseHttpsRedirection(); // Garantir que as requisições sejam feitas por HTTPS
app.UseStaticFiles(); // Permitir servir arquivos estáticos, como CSS, JS, imagens, etc.
app.UseAntiforgery(); // Proteção contra CSRF (Cross-Site Request Forgery)

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode(); // Permite renderização interativa

app.Run(); // Inicia o aplicativo