using GloboClima.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace GloboClima.API.Controllers
{
    /// <summary>
    /// Controller that provides endpoints for searching city names/autocomplete functionality.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CitySearchController : ControllerBase
    {
        private readonly CitySearchService _citySearchService;

        public CitySearchController(CitySearchService citySearchService)
        {
            _citySearchService = citySearchService;
        }

        /// <summary>
        /// Searches for cities that match the provided text query.
        /// </summary>
        /// <param name="query">Partial or full city name to search.</param>
        /// <response code="200">A list of cities matching the query.</response>
        /// <response code="400">The query parameter is missing or empty.</response>
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { message = "Query is required." });

            var cities = await _citySearchService.SearchCitiesAsync(query);
            return Ok(cities);
        }
    }
}
