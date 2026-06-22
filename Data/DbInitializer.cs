using System;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using noithat_ducanh.Data;
using noithat_ducanh.Models;

namespace noithat_ducanh.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Ensure database is created/migrated
            context.Database.EnsureCreated();

            // Seed Categories
            if (!context.Categories.Any())
            {
                var categories = new Category[]
                {
                    new() { Name = "Tủ bếp", Slug = "tu-bep" },
                    new() { Name = "Tủ quần áo", Slug = "tu-quan-ao" },
                    new() { Name = "Giường ngủ", Slug = "giuong-ngu" },
                    new() { Name = "Phòng ngủ", Slug = "phong-ngu" },
                    new() { Name = "Bàn ghế", Slug = "ban-ghe" },
                    new() { Name = "Văn phòng", Slug = "van-phong" }
                };

                context.Categories.AddRange(categories);
                context.SaveChanges();
            }

            // Seed SystemSettings (add Hotline, Address and Email if not exists)
            var settingsToSeed = new (string Key, string Value)[]
            {
                ("HeroImage1", "/images/hero-kitchen.png"),
                ("HeroImage2", "/images/hero-bedroom.jpg"),
                ("HeroImage3", "/images/hero-kitchen2.jpg"),
                ("Hotline", "0773 12 1234"),
                ("Address", "Ông Bầu, Tỉnh Đồng Tháp"),
                ("Email", "noithatnhua.ducanh@gmail.com")
            };

            foreach (var s in settingsToSeed)
            {
                if (!context.SystemSettings.Any(x => x.Key == s.Key))
                {
                    context.SystemSettings.Add(new() { Key = s.Key, Value = s.Value });
                }
            }
            context.SaveChanges();

            // Seed default SocialLinks
            if (!context.SocialLinks.Any())
            {
                var links = new SocialLink[]
                {
                    new() 
                    { 
                        Name = "Facebook", 
                        Url = "https://facebook.com/noithatducanh", 
                        Icon = "<svg viewBox=\"0 0 24 24\" aria-hidden=\"true\"><path d=\"M18 2h-3a5 5 0 00-5 5v3H7v4h3v8h4v-8h3l1-4h-4V7a1 1 0 011-1h3z\"/></svg>", 
                        IsActive = true, 
                        Order = 1 
                    },
                    new() 
                    { 
                        Name = "Zalo", 
                        Url = "https://zalo.me/0773121234", 
                        Icon = "<svg viewBox=\"0 0 24 24\" aria-hidden=\"true\"><circle cx=\"12\" cy=\"12\" r=\"10\" fill=\"#0068ff\"/><text x=\"7.5\" y=\"16.5\" font-size=\"9\" fill=\"white\" font-weight=\"bold\" font-family=\"Arial\">Z</text></svg>", 
                        IsActive = true, 
                        Order = 2 
                    },
                    new() 
                    { 
                        Name = "YouTube", 
                        Url = "https://youtube.com/c/noithatducanh", 
                        Icon = "<svg viewBox=\"0 0 24 24\" aria-hidden=\"true\"><path d=\"M22.54 6.42a2.78 2.78 0 00-1.95-1.96C18.88 4 12 4 12 4s-6.88 0-8.59.46A2.78 2.78 0 001.46 6.42 29 29 0 001 12a29 29 0 00.46 5.58 2.78 2.78 0 001.95 1.96C5.12 20 12 20 12 20s6.88 0 8.59-.46a2.78 2.78 0 001.95-1.96A29 29 0 0023 12a29 29 0 00-.46-5.58z\"/><polygon points=\"9.75 15.02 15.5 12 9.75 8.98 9.75 15.02\" fill=\"white\"/></svg>", 
                        IsActive = true, 
                        Order = 3 
                    },
                    new() 
                    { 
                        Name = "TikTok", 
                        Url = "https://tiktok.com/@noithatducanh", 
                        Icon = "<svg viewBox=\"0 0 24 24\" aria-hidden=\"true\"><path d=\"M19.59 6.69a4.83 4.83 0 01-3.77-4.25V2h-3.45v13.67a2.89 2.89 0 01-2.88 2.5 2.89 2.89 0 01-2.89-2.89 2.89 2.89 0 012.89-2.89c.28 0 .54.04.79.1V9.01a6.28 6.28 0 00-.79-.05 6.34 6.34 0 00-6.34 6.34 6.34 6.34 0 006.34 6.34 6.34 6.34 0 006.33-6.34V8.69a8.15 8.15 0 004.77 1.52V6.77a4.85 4.85 0 01-1-.08z\" fill=\"white\"/></svg>", 
                        IsActive = true, 
                        Order = 4 
                    }
                };

                context.SocialLinks.AddRange(links);
                context.SaveChanges();
            }

            // Seed Admin User
            if (!context.Admins.Any())
            {
                var admin = new Admin
                {
                    Username = "admin",
                    CreatedAt = DateTime.UtcNow
                };
                var hasher = new PasswordHasher<Admin>();
                admin.PasswordHash = hasher.HashPassword(admin, "Admin@12345");
                context.Admins.Add(admin);
                context.SaveChanges();
            }
        }
    }
}
