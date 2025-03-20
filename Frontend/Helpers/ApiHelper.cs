using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Frontend.Helpers
{
    public class ApiHelper
    {
        private readonly HttpClient _httpClient;

        // Construtor para injeção de dependência do HttpClient
        public ApiHelper(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Método para fazer requisição GET e deserializar a resposta
        public async Task<T?> GetFromApiAsync<T>(string url)
        {
            try
            {
                // Faz a requisição GET
                var response = await _httpClient.GetAsync(url);

                // Verifica se a resposta foi bem-sucedida
                response.EnsureSuccessStatusCode();

                // Deserializa e retorna o conteúdo da resposta
                return await response.Content.ReadFromJsonAsync<T>();
            }
            catch (HttpRequestException e)
            {
                // Trata exceções de requisição HTTP
                throw new ApplicationException($"Error fetching data from {url}: {e.Message}");
            }
        }
    }
}