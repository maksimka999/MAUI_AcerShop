using AcerShopBackend.Models; 
using Microsoft.EntityFrameworkCore;

namespace AcerShopBackend.Data 
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<UserCart> UserCarts { get; set; }
        public DbSet<LaptopDetails> LaptopDetails { get; set; }
        public DbSet<ChairDetails> ChairDetails { get; set; }
        public DbSet<MouseDetails> MouseDetails { get; set; } 

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserCart>()
                .HasOne(uc => uc.User)
                .WithMany(u => u.UserCarts)
                .HasForeignKey(uc => uc.UserId);

            modelBuilder.Entity<UserCart>()
                .HasOne(uc => uc.Product)
                .WithMany(p => p.UserCarts)
                .HasForeignKey(uc => uc.ProductId);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.LaptopDetails)
                .WithOne(ld => ld.Product)
                .HasForeignKey<LaptopDetails>(ld => ld.ProductId); 

            modelBuilder.Entity<Product>()
                .HasOne(p => p.ChairDetails)
                .WithOne(cd => cd.Product)
                .HasForeignKey<ChairDetails>(cd => cd.ProductId);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.MouseDetails)
                .WithOne(md => md.Product)
                .HasForeignKey<MouseDetails>(md => md.ProductId);
        }
    }
}