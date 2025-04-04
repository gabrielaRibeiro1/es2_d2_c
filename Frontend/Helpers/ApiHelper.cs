namespace Frontend.Helpers;

public class ApiHelper(HttpClient httpClient)
{
    public async Task<T?> GetFromApiAsync<T>(string url)
    {
        try
        {
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>();
        }
        catch (HttpRequestException e)
        {
            // Handle exception
            throw new ApplicationException($"Error fetching data from {url}: {e.Message}");
        }
    }
    
    public async Task<bool> PutToApiAsync<T>(string url, T data)
    {
        try
        {
            var response = await httpClient.PutAsJsonAsync(url, data);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException e)
        {
            throw new ApplicationException($"Error updating data at {url}: {e.Message}");
        }
    }


}