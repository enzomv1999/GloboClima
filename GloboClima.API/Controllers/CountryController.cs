using GloboClima.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace GloboClima.API.Controllers
{
    /// <summary>
    /// Controller that provides endpoints for retrieving country information.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CountryController : ControllerBase
    {
        private readonly CountryService _countryService;

        public CountryController(CountryService countryService)
        {
            _countryService = countryService;
        }

        /// <summary>
        /// Gets country details by ISO 3166-1 alpha-2/alpha-3 code or full country name.
        /// </summary>
        /// <param name="name">Country code or name.</param>
        /// <response code="200">Country found and returned.</response>
        /// <response code="404">No country matches the provided identifier.</response>
        [HttpGet("{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCountry(string name)
        {
            try
            {
                var result = await _countryService.GetCountryByCodeAsync(name);
                return Ok(result);
            }
            catch
            {
                return NotFound(new { message = "País não encontrado." });
            }
        }
    }
}
