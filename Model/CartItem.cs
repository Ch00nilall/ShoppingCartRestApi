using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingCartRestAPI.Model
{
    public class CartItem
    {

       public int CartItemId { get; set; }

        [Required(ErrorMessage = "Item name is required.")]
        public string Name { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        public int Quantity { get; set; }

        public byte[] ImageData { get; set; }

        public int UserId { get; set; } // Foreign key to User

        // Navigation property for the User entity
        [ForeignKey("UserId")]
        public User User { get; set; }

    }
    
} 
