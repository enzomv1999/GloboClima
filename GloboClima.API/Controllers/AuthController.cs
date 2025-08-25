using Amazon.DynamoDBv2.DataModel;
using GloboClima.API.Auth;
using GloboClima.API.DTOs;
using GloboClima.API.Models;
using GloboClima.API.Services;
using GloboClima.API.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace GloboClima.API.Controllers
{
    /// <summary>
    /// Handles user registration and authentication.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly JwtService _jwtService;

        public AuthController(UserService userService, JwtService jwtService)
        {
            _userService = userService;
            _jwtService = jwtService;
        }

        /// <summary>
        /// Registers a new user account.
        /// </summary>
        /// <param name="user">Credentials provided by the user.</param>
        /// <response code="200">User registered successfully.</response>
        /// <response code="400">Registration failed (e.g., username already exists).</response>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] UserInput user)
        {
            var (success, error) = await _userService.RegisterAsync(user.Username, user.Password);
            if (!success)
                return BadRequest(new { message = error });

            return Ok(new { message = "Usuario registrado com sucesso." });
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token.
        /// </summary>
        /// <param name="user">Credentials provided by the user.</param>
        /// <response code="200">Authentication successful.</response>
        /// <response code="401">Invalid username or password.</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] UserInput user)
        {
            var validUser = await _userService.AuthenticateAsync(user.Username, user.Password);
            if (validUser == null)
                return Unauthorized(new { message = "Credenciais Invalidas." });

            var token = _jwtService.GenerateToken(validUser.Username);
            return Ok(new { token });
        }
    }
}