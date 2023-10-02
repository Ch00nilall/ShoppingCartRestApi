using ShoppingCartRestAPI.Model;
using System.Security.Claims;

namespace ShoppingCartRestAPI.Services
{
    public interface IAUTHService
    {
        Task<ServiceResult<User>> HandleExternalLoginAsync(IEnumerable<Claim> externalClaims);

    }
}
