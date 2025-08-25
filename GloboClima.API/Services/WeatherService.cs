using GloboClima.API.Models;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;

namespace GloboClima.API.Services
{
    public class WeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public WeatherService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["OpenWeather:ApiKey"]
                      ?? Environment.GetEnvironmentVariable("OPENWEATHER_API_KEY")
                      ?? throw new InvalidOperationException("OpenWeather API key not configured");
        }

        public async Task<WeatherInfo> GetWeatherByCoordinatesAsync(double lat, double lon)
        {
            var latStr = lat.ToString(CultureInfo.InvariantCulture);
            var lonStr = lon.ToString(CultureInfo.InvariantCulture);
            var url = $"https://api.openweathermap.org/data/2.5/weather?lat={latStr}&lon={lonStr}&appid={_apiKey}&units=metric&lang=pt";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);
            var root = doc.RootElement;

            var main = root.GetProperty("main");
            var weather0 = root.GetProperty("weather")[0];
            var wind = root.TryGetProperty("wind", out var windEl) ? windEl : default;

            return new WeatherInfo
            {
                City = root.GetProperty("name").GetString() ?? string.Empty,
                CountryCode = root.GetProperty("sys").GetProperty("country").GetString() ?? string.Empty,
                Description = weather0.GetProperty("description").GetString() ?? string.Empty,
                Icon = weather0.TryGetProperty("icon", out var ic) ? ic.GetString() ?? string.Empty : string.Empty,
                Temperature = main.GetProperty("temp").GetDouble(),
                FeelsLike = main.TryGetProperty("feels_like", out var feels) ? feels.GetDouble() : 0,
                TempMin = main.TryGetProperty("temp_min", out var tmin) ? tmin.GetDouble() : 0,
                TempMax = main.TryGetProperty("temp_max", out var tmax) ? tmax.GetDouble() : 0,
                Pressure = main.TryGetProperty("pressure", out var pres) ? pres.GetInt32() : 0,
                Humidity = main.TryGetProperty("humidity", out var hum) ? hum.GetInt32() : 0,
                WindSpeed = wind.ValueKind != JsonValueKind.Undefined && wind.TryGetProperty("speed", out var ws) ? ws.GetDouble() * 3.6 : 0,
                WindDeg = wind.ValueKind != JsonValueKind.Undefined && wind.TryGetProperty("deg", out var wd) ? wd.GetInt32() : 0,
                Visibility = root.TryGetProperty("visibility", out var vis) ? vis.GetInt32() : 0,
                TimeStamp = DateTimeOffset.UtcNow
            };
        }

    }
}
