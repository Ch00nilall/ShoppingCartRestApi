using Microsoft.AspNetCore.Mvc;

namespace ShoppingCartRestAPI.Services
{
    public interface ILoginController
    {
        IActionResult Login();
        Task<IActionResult> SignInGoogleAsync();
        Task<IActionResult> Logout();
    }
}