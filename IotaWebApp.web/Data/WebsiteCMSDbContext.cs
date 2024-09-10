using Microsoft.EntityFrameworkCore;
using IotaWebApp.Models;

namespace IotaWebApp.Data
{
    public class WebsiteCMSDbContext : DbContext
    {
        public WebsiteCMSDbContext(DbContextOptions<WebsiteCMSDbContext> options) : base(options){}

        public DbSet<WebsiteContent> WebsiteContents { get; set; }
    }
}
