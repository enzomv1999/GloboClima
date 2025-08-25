using GloboClima.API.Models;
using System.Net.Http;
using System.Text.Json;

namespace GloboClima.API.Services
{
    public class CountryService
    {
        private readonly HttpClient _httpClient;

        public CountryService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CountryInfo> GetCountryByCodeAsync(string code)
        {
            var url = $"https://restcountries.com/v3.1/alpha/{code}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);

            var root = doc.RootElement[0];

            var languages = root.GetProperty("languages").EnumerateObject()
                .Select(lang => lang.Value.GetString()).ToList();

            var currencies = root.GetProperty("currencies").EnumerateObject()
                .Select(cur => cur.Value.GetProperty("name").GetString()).ToList();

            return new CountryInfo
            {
                Name = root.GetProperty("name").GetProperty("common").GetString(),
                Region = root.GetProperty("region").GetString(),
                Population = root.GetProperty("population").GetInt64(),
                Languages = languages,
                Currencies = currencies,
                FlagUrl = root.GetProperty("flags").GetProperty("png").GetString()
            };
        }
    }
}
