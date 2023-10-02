using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ShoppingCartRestAPI.Controllers;
using ShoppingCartRestAPI.Model;
using ShoppingCartRestAPI.Services;
using Xunit;

public class CartControllerTests
{
    private Mock<ICartService> _mockCartService;
    private Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private CartController _controller;
    private Mock<ILogger<LoginController>> _mockLogger;

    public CartControllerTests()
    {
        _mockCartService = new Mock<ICartService>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockLogger = new Mock<ILogger<LoginController>>();
        var mockLogger = new Mock<ILogger<CartController>>();

        _controller = new CartController(_mockCartService.Object, mockLogger.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Fact]
    public async Task GetCartItems_ReturnsOkResult()
    {
        // Arrange
        var userId = "123";
        var page = 1; // Provide the page parameter
        var pageSize = 10; // Provide the pageSize parameter

        _controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
        new Claim(ClaimTypes.NameIdentifier, userId)
        }, "mock"));

        var cartItems = new List<CartItem>
    {
        new CartItem { CartItemId = 1, UserId = 123, Name = "Item 1", Price = 10, Quantity = 2 },
        new CartItem { CartItemId = 2, UserId = 123, Name = "Item 2", Price = 15, Quantity = 3 }
    }.AsQueryable();

        _mockCartService.Setup(m => m.GetCartItemsAsync(userId, page, pageSize)).ReturnsAsync(cartItems);

        // Act
        var result = await _controller.GetCartItems(page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var items = Assert.IsAssignableFrom<IEnumerable<CartItem>>(okResult.Value);
        Assert.Equal(2, items.Count());

        // Log success message
        _mockLogger.Verify(logger => logger.LogInformation("User {UserId} requested cart items for page {Page}, pageSize {PageSize}.", userId, page, pageSize), Times.Once);
    }

    [Fact]
    public async Task CreateCartItem_ReturnsCreatedResult()
    {
        // Arrange
        var userId = "123";
        _controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
        new Claim(ClaimTypes.NameIdentifier, userId)
        }, "mock"));

        var cartItem = new CartItem { Name = "New Item", Price = 10, Quantity = 2 };
        var imageFile = new FormFile(new MemoryStream(), 0, 0, "Image", "image.jpg"); // Provide an IFormFile instance

        _mockCartService
            .Setup(m => m.CreateCartItemAsync(userId, cartItem, imageFile))
            .Returns(Task.FromResult(new ServiceResult<CartItem>(cartItem, true, null)));

        // Act
        var result = await _controller.CreateCartItem(cartItem, imageFile);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        var createdCartItem = Assert.IsType<CartItem>(createdAtActionResult.Value);
        Assert.Equal("New Item", createdCartItem.Name);
        Assert.Equal(123, createdCartItem.UserId);

        // Log success message
        _mockLogger.Verify(logger => logger.LogInformation("User {UserId} created a new cart item.", userId), Times.Once);
    }

    [Fact]
    public async Task UpdateCartItem_ReturnsNoContent()
    {
        // Arrange
        var userId = "123";
        _controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
        new Claim(ClaimTypes.NameIdentifier, userId)
        }, "mock"));

        var itemId = 1;
        var updatedCartItem = new CartItem { Name = "Updated Item", Price = 20, Quantity = 4 };

        var existingCartItem = new CartItem { CartItemId = itemId, UserId = 123, Name = "Item 1", Price = 10, Quantity = 2 };
        _mockCartService.Setup(m => m.GetCartItemAsync(userId, itemId)).ReturnsAsync(existingCartItem);
        _mockCartService.Setup(m => m.UpdateCartItemAsync(userId, itemId, updatedCartItem)).ReturnsAsync(new ServiceResult<CartItem>(updatedCartItem, true, null));

        // Act
        var result = await _controller.UpdateCartItem(itemId, updatedCartItem);

        // Assert
        Assert.IsType<NoContentResult>(result);
        Assert.Equal("Updated Item", existingCartItem.Name);
        Assert.Equal(20, existingCartItem.Price);
        Assert.Equal(4, existingCartItem.Quantity);

        // Log success message
        _mockLogger.Verify(logger => logger.LogInformation("User {UserId} updated a existing cart item.", userId), Times.Once);

    }

    [Fact]
    public async Task DeleteCartItem_ReturnsNoContent()
    {
        // Arrange
        var userId = "123";
        _controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
        new Claim(ClaimTypes.NameIdentifier, userId)
        }, "mock"));

        var itemId = 1;
        var existingCartItem = new CartItem { CartItemId = itemId, UserId = 123, Name = "Item 1", Price = 10, Quantity = 2 };
        _mockCartService.Setup(m => m.GetCartItemAsync(userId, itemId)).ReturnsAsync(existingCartItem);
        _mockCartService.Setup(m => m.DeleteCartItemAsync(userId, itemId)).ReturnsAsync(new ServiceResult<CartItem>(null, true, null));

        // Act
        var result = await _controller.DeleteCartItem(itemId);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockCartService.Verify(m => m.DeleteCartItemAsync(userId, itemId), Times.Once);
    }
    private static byte[] GetSampleImageData()
    {
        // This creates a sample byte array representing a simple image (e.g., a white pixel)
        var pixelData = new byte[] { 255, 255, 255 }; // RGB values for a white pixel
        return pixelData;
    }

    private static List<ValidationResult> ValidateModel(object model)
    {
        var validationContext = new ValidationContext(model, null, null);
        var validationResults = new List<ValidationResult>();
        Validator.TryValidateObject(model, validationContext, validationResults, true);
        return validationResults;
    }
    [Fact]
    public void NameIsRequired()
    {
        // Arrange
        var cartItem = new CartItem();

        // Act
        var validationResults = ValidateModel(cartItem);

        // Assert
        var nameRequiredError = validationResults.SingleOrDefault(vr => vr.MemberNames.Contains("Name"));
        Assert.NotNull(nameRequiredError);
        Assert.Equal("Item name is required.", nameRequiredError.ErrorMessage);
    }

    [Fact]
    public void PriceMustBeGreaterThanZero()
    {
        // Arrange
        var cartItem = new CartItem { Name = "Item 1", Price = 0.0m, Quantity = 2 };

        // Act
        var validationResults = ValidateModel(cartItem);

        // Assert
        var priceRangeError = validationResults.SingleOrDefault(vr => vr.MemberNames.Contains("Price"));
        Assert.Equal("Price must be greater than 0.", priceRangeError.ErrorMessage);
    }

    [Fact]
    public void QuantityMustBeGreaterThanZero()
    {
        // Arrange
        var cartItem = new CartItem { Name = "Item 1", Price = 10.0m, Quantity = 0 };

        // Act
        var validationResults = ValidateModel(cartItem);

        // Assert
        var quantityRangeError = validationResults.SingleOrDefault(vr => vr.MemberNames.Contains("Quantity"));
        Assert.Equal("Quantity must be greater than 0.", quantityRangeError.ErrorMessage);
    }

    [Fact]
    public void ImageDataIsValid()
    {
        // Arrange
        var cartItem = new CartItem
        {
            Name = "Item 1",
            Price = 10.0m,
            Quantity = 2,
            ImageData = GetSampleImageData()
        };

        // Act
        var validationResults = ValidateModel(cartItem);

        // Assert
        Assert.Empty(validationResults);
    }

}
