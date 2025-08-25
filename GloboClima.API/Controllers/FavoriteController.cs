using FluentValidation;
using FluentValidation.Results;
using GloboClima.API.DTOs;
using GloboClima.API.Exceptions;
using GloboClima.API.Models;
using GloboClima.API.Services;
using GloboClima.API.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace GloboClima.API.Controllers
{
    /// <summary>
    /// Controller responsible for CRUD operations over user favorite items.
    /// Requires authentication via JWT.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FavoriteController : ControllerBase
    {
        private readonly FavoriteService _favoriteService;
        private readonly IValidator<FavoriteInput> _validator;
        private readonly ILogger<FavoriteController> _logger;

        public FavoriteController(
            FavoriteService favoriteService, 
            IValidator<FavoriteInput> validator,
            ILogger<FavoriteController> logger)
        {
            _favoriteService = favoriteService ?? throw new ArgumentNullException(nameof(favoriteService));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates a new favorite for the authenticated user.
        /// </summary>
        /// <param name="input">Data for the favorite to be saved.</param>
        /// <response code="201">Favorite successfully created.</response>
        /// <response code="400">Validation failed for input.</response>
        /// <response code="401">User is not authenticated.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Save([FromBody] FavoriteInput input)
        {
            _logger.LogInformation("Saving favorite for user {Username}", User.Identity?.Name);
            
            var validationResult = await _validator.ValidateAsync(input);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(v => v.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(x => x.ErrorMessage).ToArray()
                    );
                _logger.LogWarning("Validation failed for favorite input: {Errors}", errors);
                throw new Exceptions.ValidationException(errors);
            }

            try
            {
                var favorite = new Favorite
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = User.Identity?.Name ?? throw new UnauthorizedAccessException("User not authenticated"),
                    Type = input.Type,
                    Name = input.Name,
                    CreatedAt = DateTime.UtcNow
                };

                await _favoriteService.SaveAsync(favorite);
                _logger.LogInformation("Favorite {FavoriteId} saved successfully", favorite.Id);
                
                return CreatedAtAction(nameof(GetAll), new { id = favorite.Id }, favorite);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving favorite");
                throw new ApiException(HttpStatusCode.InternalServerError, "An error occurred while saving the favorite");
            }
        }

       
        /// <summary>
        /// Retrieves all favorites belonging to the authenticated user.
        /// </summary>
        /// <response code="200">A list of favorites returned.</response>
        /// <response code="401">User is not authenticated.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll()
        {
            var username = User.Identity?.Name ?? throw new UnauthorizedAccessException("User not authenticated");
            _logger.LogInformation("Retrieving favorites for user {Username}", username);
            
            try
            {
                var favorites = await _favoriteService.GetAllAsync(username);
                return Ok(favorites);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving favorites for user {Username}", username);
                throw new ApiException(HttpStatusCode.InternalServerError, "An error occurred while retrieving favorites");
            }
        }

       
        /// <summary>
        /// Deletes a favorite by its identifier.
        /// </summary>
        /// <param name="id">Identifier of the favorite.</param>
        /// <response code="204">Favorite deleted.</response>
        /// <response code="404">Favorite not found.</response>
        /// <response code="401">User not authenticated.</response>
        /// <response code="403">User not allowed to delete this favorite.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Delete([Required] string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                _logger.LogWarning("Delete favorite called with empty ID");
                return BadRequest("Favorite ID is required");
            }

            var username = User.Identity?.Name ?? throw new UnauthorizedAccessException("User not authenticated");
            _logger.LogInformation("Deleting favorite {FavoriteId} for user {Username}", id, username);
            
            try
            {
                var favorite = await _favoriteService.GetByIdAsync(id);
                if (favorite == null)
                {
                    _logger.LogWarning("Favorite {FavoriteId} not found for deletion", id);
                    return NotFound(new { message = $"Favorite with ID {id} not found" });
                }

                if (favorite.Username != username)
                {
                    _logger.LogWarning("User {Username} attempted to delete favorite {FavoriteId} belonging to {Owner}", 
                        username, id, favorite.Username);
                    return Forbid();
                }

                await _favoriteService.DeleteAsync(id);
                _logger.LogInformation("Favorite {FavoriteId} deleted successfully", id);
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting favorite {FavoriteId}", id);
                throw new ApiException(HttpStatusCode.InternalServerError, $"An error occurred while deleting the favorite: {ex.Message}");
            }
        }
    }
}
