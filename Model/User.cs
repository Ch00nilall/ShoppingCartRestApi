using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ShoppingCartRestAPI.Model
{
    public class User
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        // Navigation property for the CartItem collection
        // Prevent serialization loop
        [JsonIgnore] 
        public ICollection<CartItem> CartItems { get; set; }
    }
}
