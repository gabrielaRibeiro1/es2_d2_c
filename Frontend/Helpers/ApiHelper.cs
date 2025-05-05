using System.Net;

namespace Frontend.Helpers;

public class ApiHelper
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiHelper> _logger;

    public ApiHelper(HttpClient httpClient, ILogger<ApiHelper> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<T?> GetFromApiAsync<T>(string endpoint)
    {
        try
        {
            _logger.LogInformation($"Fetching data from {endpoint}");
            var response = await _httpClient.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"API Error: {response.StatusCode} - {errorContent}");
                throw new ApiException(response.StatusCode, errorContent);
            }

            return await response.Content.ReadFromJsonAsync<T>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in GetFromApiAsync for endpoint {endpoint}");
            throw;
        }
    }

    public async Task<TResponse?> PostToApiAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        try
        {
            _logger.LogInformation($"Posting data to {endpoint}");
            var response = await _httpClient.PostAsJsonAsync(endpoint, data);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"API Error: {response.StatusCode} - {errorContent}");
                throw new ApiException(response.StatusCode, errorContent);
            }

            return await response.Content.ReadFromJsonAsync<TResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in PostToApiAsync for endpoint {endpoint}");
            throw;
        }
    }

    public async Task<TResponse?> PutToApiAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        try
        {
            _logger.LogInformation($"Putting data to {endpoint}");
            var response = await _httpClient.PutAsJsonAsync(endpoint, data);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"API Error: {response.StatusCode} - {errorContent}");
                throw new ApiException(response.StatusCode, errorContent);
            }

            return await response.Content.ReadFromJsonAsync<TResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in PutToApiAsync for endpoint {endpoint}");
            throw;
        }
    }

    public async Task<bool> DeleteFromApiAsync(string endpoint)
    {
        try
        {
            _logger.LogInformation($"Deleting resource at {endpoint}");
            var response = await _httpClient.DeleteAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"API Error: {response.StatusCode} - {errorContent}");
                throw new ApiException(response.StatusCode, errorContent);
            }

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in DeleteFromApiAsync for endpoint {endpoint}");
            throw;
        }
    }
}

public class ApiException : Exception
{
    public HttpStatusCode StatusCode { get; }

    public ApiException(HttpStatusCode statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }
}