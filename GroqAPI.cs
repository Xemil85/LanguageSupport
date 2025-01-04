using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class GroqAPI
{
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private readonly HttpClient _httpClient;

    public GroqAPI(IConfiguration configuration)
    {
        _apiKey = configuration["GroqApi:Key"];
        _baseUrl = configuration["GroqApi:BaseUrl"];
        _httpClient = new HttpClient();
    }

    public async Task<string> GetChatResponseAsync(string userInput)
    {
        var url = _baseUrl;
        var prompt = $"Muunna lause yksinkertaisempaan muotoon: {userInput}";
        var payload = new
        {
            model = "gemma2-9b-it", // Malli
            messages = new[]
            {
                new { role = "user", content = prompt } // Viestin sisältö
            }
        };

        var jsonPayload = JsonConvert.SerializeObject(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        // Lisää Authorization-header
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

        var response = await _httpClient.PostAsync(url, content);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"API call failed: {response.StatusCode}, {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        dynamic jsonResponse = JsonConvert.DeserializeObject(responseContent);

        // Palauta vastauksen sisältö
        return jsonResponse.choices[0].message.content.ToString();
    }
}
