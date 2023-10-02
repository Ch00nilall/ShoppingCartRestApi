using ShoppingCartRestAPI.Model;

namespace ShoppingCartRestAPI.Services
{
    public interface ICartService
    {
        Task<IEnumerable<CartItem>> GetCartItemsAsync(string userId, int page, int pageSize);

        Task<ServiceResult<CartItem>> CreateCartItemAsync(string userId, CartItem cartItem, IFormFile image);

        Task<ServiceResult<CartItem>> UpdateCartItemAsync(string userId, int itemId, CartItem updatedCartItem);

        Task<ServiceResult<CartItem>> DeleteCartItemAsync(string userId, int itemId);

        Task<CartItem> GetCartItemAsync(string userId, int itemId);
    }
}
