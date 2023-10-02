using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ShoppingCartRestAPI.Data;
using ShoppingCartRestAPI.Services;
using System.Security.Claims;
using Xunit;
using Microsoft.Extensions.Logging;


public class LoginControllerTests
{
    private Mock<IAUTHService> _mockAuthService;
    private Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private Mock<ILogger<LoginController>> _mockLogger;
    private LoginController _controller;


    public LoginControllerTests()
    {
        _mockAuthService = new Mock<IAUTHService>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockLogger = new Mock<ILogger<LoginController>>();

        _controller = new LoginController(_mockAuthService.Object, _mockLogger.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Fact]
    public async Task Login_ReturnsRedirectToGoogleLogin()
    {
        // Arrange
        var redirectUri = "/signin-google";

        // Act
        var result = _controller.Login() as ChallengeResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(redirectUri, result.Properties.RedirectUri);

        // Log a message
        _mockLogger.Verify(logger => logger.LogInformation("Login method called."), Times.Once);
    }

    [Fact]
    public async Task SignInGoogleAsync_ReturnsOkResultOnSuccessfulAuthentication()
    {
        // Arrange
        var userClaims = new Claim[] {
        new Claim(ClaimTypes.Email, "test@example.com"),
        new Claim(ClaimTypes.NameIdentifier, "123"),
        new Claim(ClaimTypes.Name, "Test User")
    };

        var authenticateResult = AuthenticateResult.Success(new AuthenticationTicket(
            new ClaimsPrincipal(new ClaimsIdentity(userClaims, GoogleDefaults.AuthenticationScheme)),
            GoogleDefaults.AuthenticationScheme));

        _mockHttpContextAccessor.Setup(m => m.HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme))
            .ReturnsAsync(authenticateResult);

        // Act
        var result = await _controller.SignInGoogleAsync();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Welcome, test@example.com!", okResult.Value);

        // Log success message
        _mockLogger.Verify(logger => logger.LogInformation("User test@example.com successfully logged in."), Times.Once);
    }

    [Fact]
    public async Task SignInGoogleAsync_ReturnsInternalServerErrorOnDatabaseError()
    {
        // Arrange
        var userClaims = new Claim[] {
        new Claim(ClaimTypes.Email, "test@example.com"),
        new Claim(ClaimTypes.NameIdentifier, "123"),
        new Claim(ClaimTypes.Name, "Test User")
    };

        var authenticateResult = AuthenticateResult.Success(new AuthenticationTicket(
            new ClaimsPrincipal(new ClaimsIdentity(userClaims, GoogleDefaults.AuthenticationScheme)),
            GoogleDefaults.AuthenticationScheme));

        _mockHttpContextAccessor.Setup(m => m.HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme))
            .ReturnsAsync(authenticateResult);

        // Act
        var result = await _controller.SignInGoogleAsync();

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("Internal server error: Database error", statusCodeResult.Value);

        // Log error message
        _mockLogger.Verify(logger => logger.LogError("External login failed: Database error"), Times.Once);
    }
}