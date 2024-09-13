using IotaWebApp.Data;
using IotaWebApp.Models;
using System.Linq;

namespace IotaWebApp.Data
{
    public static class DbInitializer
    {
        public static void Initialize(WebsiteCMSDbContext context)
        {
            // Ensure the database is created
            context.Database.EnsureCreated();

            // Check if the database already has data to avoid seeding twice
            if (context.WebsiteContents.Any())
            {
                return; // Database has already been seeded
            }

            var contents = new WebsiteContent[]
            {
                new WebsiteContent { ContentKey = "HeroImage", ContentValue = "~/Assets/image4.png" },
                new WebsiteContent { ContentKey = "HeroTitle", ContentValue = "Creating Exceptional Workspaces" },
                new WebsiteContent { ContentKey = "HeroButtonText", ContentValue = "Explore Our Work" }
            };

            foreach (var content in contents)
            {
                context.WebsiteContents.Add(content);
            }

            context.SaveChanges();
        }
    }
}
