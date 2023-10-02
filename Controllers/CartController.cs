using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ShoppingCartRestAPI.Data;
using ShoppingCartRestAPI.Model;
using ShoppingCartRestAPI.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingCartRestAPI.Controllers
{
    [ApiController]
    [Route("api/cart")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartService cartService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        [HttpGet("items")]
        [Authorize]
        public async Task<IActionResult> GetCartItems(int page = 1, int pageSize = 10)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            try
            {
                var cartItems = await _cartService.GetCartItemsAsync(userId, page, pageSize);

                _logger.LogInformation($"GetCartItems requested by user {userId}.");

                return Ok(cartItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing GetCartItems.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("items")]
        [Authorize]
        public async Task<IActionResult> CreateCartItem([FromBody] CartItem cartItem, IFormFile image)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("CreateCartItem ModelState validation failed.");
                    return BadRequest(ModelState);
                }

                var result = await _cartService.CreateCartItemAsync(userId, cartItem, image);
                if (result.Success)
                {
                    _logger.LogInformation($"CreateCartItem completed successfully for user {userId}.");
                    return CreatedAtAction(nameof(GetCartItems), new { id = result.Data.CartItemId }, result.Data);
                }
                else
                {
                    _logger.LogWarning($"CreateCartItem failed for user {userId}: {result.Message}");
                    return BadRequest(result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing CreateCartItem.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("items/{itemId}")]
        [Authorize]
        public async Task<IActionResult> UpdateCartItem(int itemId, [FromBody] CartItem updatedCartItem)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("UpdateCartItem ModelState validation failed.");
                    return BadRequest(ModelState);
                }

                var result = await _cartService.UpdateCartItemAsync(userId, itemId, updatedCartItem);
                if (result.Success)
                {
                    _logger.LogInformation($"UpdateCartItem completed successfully for user {userId}.");
                    return NoContent();
                }
                else
                {
                    _logger.LogWarning($"UpdateCartItem failed for user {userId}: {result.Message}");
                    return BadRequest(result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing UpdateCartItem.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("items/{itemId}")]
        [Authorize]
        public async Task<IActionResult> DeleteCartItem(int itemId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            try
            {
                var result = await _cartService.DeleteCartItemAsync(userId, itemId);
                if (result.Success)
                {
                    _logger.LogInformation($"DeleteCartItem completed successfully for user {userId}.");
                    return NoContent();
                }
                else
                {
                    _logger.LogWarning($"DeleteCartItem failed for user {userId}: {result.Message}");
                    return BadRequest(result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing DeleteCartItem.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
