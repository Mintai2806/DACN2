using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;

namespace Shopping_Tutorial.Repository
{
    public class SeedData
    {
        public static void SeedingData(DataContext _context)
        {
            _context.Database.Migrate();
            if (!_context.Products.Any())
            {
                CategoryModel macbook = new CategoryModel
                {
                    Name = "Macbook",
                    Slug = "Macbook",
                    Description = "Macbook is brand in the world",
                    Status = 1
                };

                CategoryModel pc = new CategoryModel
                {
                    Name = "PC",
                    Slug = "pc",
                    Description = "pc is brand in the world",
                    Status = 1
                };

				BrandModel apple = new BrandModel
				{
					Name = "Apple",
					Slug = "apple",
					Description = "apple is brand in the world",
					Status = 1
				};

				BrandModel samsung = new BrandModel
				{
					Name = "Samsung",
					Slug = "samsung",
					Description = "samsung is brand in the world",
					Status = 1
				};

				_context.Products.AddRange(
                    new ProductModel
                    {
                        Name = "Macbook",
                        Slug = "Macbook",
                        Description = "Macbook is the best ",
                        Image = "1.jpg",
                        Category = macbook,
                        Brand = apple,
                        Price = 20000,
                        CapitalPrice = 2900,
                        Quantity = 100
                    },

                    new ProductModel
                    {
                        Name = "pc",
                        Slug = "pc",
                        Description = "pc is the best ",
                        Image = "1.jpg",
                        Category = pc,
                        Brand = samsung,
                        Price = 30000,
                        CapitalPrice = 2900,
                        Quantity = 200
                    }
                );
                _context.SaveChanges();
            }
            if (!_context.Contact.Any())
            {
                ContactModel contact = new ContactModel
                {
                    Id = 1,
                    Name = "Shop Bán Hàng Asp.net",
                    Description = "Mô tả dành cho Shop Bán Hàng Asp.Net",
                    Phone = "123456789",
                    Email = "admin@gmail.com",
                    Map = "Map Embeb Iframe Here",
                    LogoImg = "logo.jpg"
                };
                // Add the contact to the database context
                _context.Contact.Add(contact);
                _context.SaveChanges();
            }
            if (!_context.Roles.Any())
            {
                // Create roles
                var roles = new List<IdentityRole>
                    {
                        new IdentityRole { Name = "Admin", NormalizedName = "ADMIN" },
                        new IdentityRole { Name = "Author", NormalizedName = "AUTHOR" },
                        new IdentityRole { Name = "Publisher", NormalizedName = "PUBLISHER" }
                    };

                foreach (var role in roles)
                {
                    _context.Roles.Add(role);
                }

                _context.SaveChanges();
            }
            if (!_context.Users.Any())
            {
                // Create a new user
                var user = new AppUserModel
                {
                    UserName = "admin",
                    Email = "admin@gmail.com",
                    EmailConfirmed = true, // Skip email confirmation
                    NormalizedUserName = "ADMIN",
                    NormalizedEmail = "ADMIN@GMAIL.COM",

                    PasswordHash = new PasswordHasher<AppUserModel>().HashPassword(null, "Ta!123"), // Securely hash the password
                    SecurityStamp = Guid.NewGuid().ToString(), // Generate a unique security stamp
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };

                // Add user to the context
                _context.Users.Add(user);

                // Ensure the role exists in the database
                var roleName = "Admin";
                var role = _context.Roles.FirstOrDefault(r => r.Name == roleName);
                if (role == null)
                {
                    role = new IdentityRole
                    {
                        Name = roleName,
                        NormalizedName = roleName.ToUpper()
                    };
                    _context.Roles.Add(role);
                    _context.SaveChanges();
                }
                // Assign the user to the role
                var userRole = new IdentityUserRole<string>
                {
                    UserId = user.Id,
                    RoleId = role.Id
                };
                _context.UserRoles.Add(userRole);
                // Save changes to the database
                _context.SaveChanges();

                Console.WriteLine("User and role assignment seeded successfully!");
            }
            else
            {
                Console.WriteLine("Users already exist in the database.");
            }
        }
    }
}
