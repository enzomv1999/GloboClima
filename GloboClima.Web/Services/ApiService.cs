using Blazored.LocalStorage;
using GloboClima.Web.Models;
using Microsoft.AspNetCore.WebUtilities;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;


namespace GloboClima.Web.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private string? _jwtToken;
        public event Action? OnAuthStateChanged;
        public bool IsAuthenticated => !string.IsNullOrEmpty(_jwtToken);
        public UserInfo? CurrentUser { get; private set; }


        public ApiService(IHttpClientFactory httpClientFactory, ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
            _httpClient = httpClientFactory.CreateClient("API");
        }

        public async Task SetToken(string token)
        {
            _jwtToken = token;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            await _localStorage.SetItemAsync("jwt", token);
            OnAuthStateChanged?.Invoke();
        }

        public async Task LoadToken()
        {
            if (_localStorage == null)
            {
                Console.WriteLine("[DEBUG] LocalStorage ainda não injetado.");
                return;
            }

            try
            {
                var token = await _localStorage.GetItemAsync<string>("jwt");

                if (!string.IsNullOrEmpty(token))
                {
                    _jwtToken = token;
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    OnAuthStateChanged?.Invoke();
                }
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex.GetType().Name == "JSException")
            {
                Console.WriteLine($"[DEBUG] LoadToken skipped (prerender/JS unavailable): {ex.Message}");
            }
        }

        public async Task ClearToken()
        {
            _jwtToken = null;
            _httpClient.DefaultRequestHeaders.Authorization = null;
            await _localStorage.RemoveItemAsync("jwt");
        }

        public string? GetToken()
        {
            return _jwtToken;
        }

        public async Task<string?> Login(User user)
        {
            try
            {
                var json = JsonSerializer.Serialize(user);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("auth/login", content);

                if (!response.IsSuccessStatusCode)
                    return null;

                var jsonResponse = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(jsonResponse);
                var root = document.RootElement;

                if (!root.TryGetProperty("token", out var tokenProperty) || tokenProperty.GetString() is not string token)
                {
                    return null;
                }

                CurrentUser = new UserInfo { Name = user.Username };

                return token;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during login: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> Register(User user)
        {
            try
            {
                var json = JsonSerializer.Serialize(new { user.Username, user.Password });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("auth/register", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Registration failed: {response.StatusCode} - {errorContent}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during registration: {ex.Message}");
                return false;
            }
        }

        public async Task Logout()
        {
            _jwtToken = null;
            CurrentUser = null;
            _httpClient.DefaultRequestHeaders.Authorization = null;
            await _localStorage.RemoveItemAsync("jwt");
            OnAuthStateChanged?.Invoke();
        }

        public async Task<bool> AddFavorite(string type, string name)
        {
            var payload = new
            {
                Type = type,
                Name = name
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("Favorite", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<List<Favorite>> GetFavorites()
        {
            var response = await _httpClient.GetAsync("favorite");
            if (!response.IsSuccessStatusCode) return new();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Favorite>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                   ?? new List<Favorite>();
        }

        public async Task<bool> DeleteFavorite(string id)
        {
            var response = await _httpClient.DeleteAsync($"favorite/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<GeocodingResult>> SearchCities(string query, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"citysearch/search?query={Uri.EscapeDataString(query)}", cancellationToken);
                if (!response.IsSuccessStatusCode) return new List<GeocodingResult>();

                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<List<GeocodingResult>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new List<GeocodingResult>();
            }
            catch (OperationCanceledException)
            {
                return new List<GeocodingResult>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching cities: {ex.Message}");
                return new List<GeocodingResult>();
            }
        }

        public async Task<WeatherAndCountry?> GetWeatherAndCountry(double lat, double lon)
        {
            var queryParams = new Dictionary<string, string?>
            {
                { "lat", lat.ToString(CultureInfo.InvariantCulture) },
                { "lon", lon.ToString(CultureInfo.InvariantCulture) }
            };

            var url = QueryHelpers.AddQueryString("weather/full", queryParams);

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<WeatherAndCountry>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }


    }
}
