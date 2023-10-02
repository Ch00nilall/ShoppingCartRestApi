using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging; // Add this import
using System.Security.Claims;
using ShoppingCartRestAPI.Model;
using ShoppingCartRestAPI.Data;
using System.Threading.Tasks;
using System;
using ShoppingCartRestAPI.Services;

[Route("api/login")]
[ApiController]
public class LoginController : ControllerBase, ILoginController
{
    private readonly IAUTHService _authService;
    private readonly ILogger<LoginController> _logger; // Inject ILogger

    public LoginController(IAUTHService authService, ILogger<LoginController> logger)
    {
        _authService = authService;
        _logger = logger;  
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        try
        {
            var redirectUri = "/signin-google";
            return Challenge(new AuthenticationProperties { RedirectUri = redirectUri }, GoogleDefaults.AuthenticationScheme);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login.");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("signin-google")]
    public async Task<IActionResult> SignInGoogleAsync()
    {
        try
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (authenticateResult.Succeeded)
            {
                // User has been successfully authenticated with Google.
                var claims = authenticateResult.Principal.Claims;
                var result = await _authService.HandleExternalLoginAsync(claims);

                if (result.Success)
                {
                    _logger.LogInformation($"User {result.Data.Username} successfully logged in.");
                    return Ok($"Welcome, {result.Data.Username}!");
                }
                else
                {
                    _logger.LogError($"External login failed: {result.Message}");
                    return BadRequest(result.Message);
                }
            }
            else if (authenticateResult.Failure != null)
            {
                _logger.LogError($"Authentication failed: {authenticateResult.Failure}");
                return BadRequest($"Authentication failed: {authenticateResult.Failure}");
            }
            else
            {
                _logger.LogError("Authentication failed for an unknown reason.");
                return BadRequest("Authentication failed for an unknown reason.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Google sign-in.");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        try
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation("User logged out.");
            return Ok("Logged out");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout.");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
