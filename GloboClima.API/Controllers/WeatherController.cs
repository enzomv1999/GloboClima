using GloboClima.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace GloboClima.API.Controllers
{
    /// <summary>
    /// Controller providing weather information based on geographic coordinates and related country data.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherController : ControllerBase
    {
        private readonly WeatherService _weatherService;
        private readonly CountryService _countryService;

        public WeatherController(WeatherService weatherService, CountryService countryService)
        {
            _weatherService = weatherService;
            _countryService = countryService;
        }

        /// <summary>
        /// Returns current weather data for a latitude/longitude pair.
        /// </summary>
        /// <param name="lat">Latitude of the location.</param>
        /// <param name="lon">Longitude of the location.</param>
        /// <response code="200">Weather information successfully retrieved.</response>
        /// <response code="404">Weather data not found for the coordinates.</response>
        /// <response code="400">Invalid latitude or longitude supplied.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetWeather([FromQuery] double lat, [FromQuery] double lon)
        {
            try
            {
                var result = await _weatherService.GetWeatherByCoordinatesAsync(lat, lon);
                return Ok(result);
            }
            catch
            {
                return NotFound(new { message = "Weather data not found." });
            }
        }

        /// <summary>
        /// Returns weather information and its corresponding country details for a latitude/longitude pair.
        /// </summary>
        /// <param name="lat">Latitude of the location.</param>
        /// <param name="lon">Longitude of the location.</param>
        /// <response code="200">Combined weather and country information returned.</response>
        /// <response code="404">Weather or country data not found.</response>
        /// <response code="400">Invalid latitude or longitude supplied.</response>
        [HttpGet("full")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetWeatherAndCountry([FromQuery] double lat, [FromQuery] double lon)
        {
            var weather = await _weatherService.GetWeatherByCoordinatesAsync(lat, lon);
            if (weather == null)
                return NotFound("Weather not found");

            var country = await _countryService.GetCountryByCodeAsync(weather.CountryCode);
            if (country == null)
                return NotFound("Country not found");

            return Ok(new
            {
                weather,
                country
            });
        }
    }
}
