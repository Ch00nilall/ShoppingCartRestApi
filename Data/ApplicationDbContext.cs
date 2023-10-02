using Microsoft.EntityFrameworkCore;
using ShoppingCartRestAPI.Model;

namespace ShoppingCartRestAPI.Data
{

    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<User> Users { get; set; } 
    }
}