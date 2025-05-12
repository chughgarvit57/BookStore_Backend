using Microsoft.EntityFrameworkCore;
using ModelLayer.Entity;

namespace RepositoryLayer.Context
{
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions<UserContext> options) : base(options)
        {
        }
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<BookEntity> Books { get; set; }
        public DbSet<AddressEntity> Addresses { get; set; }
        public DbSet<WishListEntity> WishList { get; set; }
        public DbSet<CartEntity> Cart { get; set; }
        public DbSet<OrderEntity> Orders { get; set; }
    }
}
