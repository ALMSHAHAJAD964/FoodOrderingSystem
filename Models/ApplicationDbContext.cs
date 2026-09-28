using Microsoft.EntityFrameworkCore;

namespace FoodOrderingSystem.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options
        ) : base(options)
        {
        }

        // Tables
        public DbSet<FoodItem> FoodItems { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Order -> OrderItems
            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // OrderItem -> FoodItem
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.FoodItem)
                .WithMany()
                .HasForeignKey(oi => oi.FoodItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // Category -> FoodItems
            modelBuilder.Entity<Category>()
                .HasMany(c => c.FoodItems)
                .WithOne(f => f.Category)
                .HasForeignKey(f => f.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Category Seed Data
            modelBuilder.Entity<Category>().HasData(
                new Category
                {
                    Id = 1,
                    Name = "Pizza",
                    Description = "Delicious pizzas with various toppings."
                },
                new Category
                {
                    Id = 2,
                    Name = "Burgers",
                    Description = "Juicy burgers with fresh ingredients."
                },
                new Category
                {
                    Id = 3,
                    Name = "Pasta",
                    Description = "Tasty pasta dishes with rich sauces."
                }
            );

            // FoodItem Seed Data
            modelBuilder.Entity<FoodItem>().HasData(
                new FoodItem
                {
                    Id = 1,
                    Name = "Margherita Pizza",
                    Description = "Classic pizza with tomato sauce and mozzarella cheese.",
                    Price = 9.99m,
                    CategoryId = 1,
                    ImageUrl= "https://www.themealdb.com/images/media/meals/sutysw1468247559.jpg"
                },
                new FoodItem
                {
                    Id = 2,
                    Name = "Pepperoni Pizza",
                    Description = "Pizza topped with pepperoni slices.",
                    Price = 11.99m,
                    CategoryId = 1,
                    ImageUrl= "https://www.themealdb.com/images/media/meals/sutysw1468247559.jpg"
                },
                new FoodItem
                {
                    Id = 3,
                    Name = "Cheeseburger",
                    Description = "Juicy beef patty with cheese, lettuce, and tomato.",
                    Price = 8.99m,
                    CategoryId = 2,
                    ImageUrl = "https://www.themealdb.com/images/media/meals/sutysw1468247559.jpg"
                },
                new FoodItem
                {
                    Id = 4,
                    Name = "Veggie Burger",
                    Description = "Delicious vegetarian burger with fresh vegetables.",
                    Price = 7.99m,
                    CategoryId = 2,
                    ImageUrl = "https://www.themealdb.com/images/media/meals/sutysw1468247559.jpg"
                },
                new FoodItem
                {
                    Id = 5,
                    Name = "Spaghetti Bolognese",
                    Description = "Classic Italian pasta dish with meat sauce.",
                    Price = 10.99m,
                    CategoryId = 3,
                    ImageUrl = "https://www.themealdb.com/images/media/meals/sutysw1468247559.jpg"
                },
                new FoodItem
                {
                    Id = 6,
                    Name = "Fettuccine Alfredo",
                    Description = "Creamy pasta dish with Alfredo sauce.",
                    Price = 12.99m,
                    CategoryId = 3,
                    ImageUrl = "https://www.themealdb.com/images/media/meals/sutysw1468247559.jpg"
                }
            );

            // Admin User Seed Data
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "John Doe",
                    Email = "Admin@gmail.com",
                    Password = "Admin@123",
                    IsAdmin = true
                }
            );
        }
    }
}