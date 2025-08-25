using GloboClima.API.Models;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace GloboClima.API.Services
{
    public class CitySearchService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public CitySearchService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["OpenWeather:ApiKey"]
                     ?? Environment.GetEnvironmentVariable("OPENWEATHER_API_KEY")
                     ?? throw new InvalidOperationException("OpenWeather API key not configured");
        }

        public async Task<List<CitySearchResult>> SearchCitiesAsync(string query)
        {
            var q = Uri.EscapeDataString(query);
            var url = $"https://api.openweathermap.org/geo/1.0/direct?q={q}&limit=5&appid={_apiKey}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync();
            var json = await JsonDocument.ParseAsync(stream);
            var results = new List<CitySearchResult>();

            foreach (var item in json.RootElement.EnumerateArray())
            {
                results.Add(new CitySearchResult
                {
                    Name = item.GetProperty("name").GetString() ?? string.Empty,
                    Country = item.GetProperty("country").GetString() ?? string.Empty,
                    State = item.TryGetProperty("state", out var state) ? (state.GetString() ?? string.Empty) : string.Empty,
                    Latitude = item.GetProperty("lat").GetDouble(),
                    Longitude = item.GetProperty("lon").GetDouble()
                });
            }

            return results;
        }
    }
}
