using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using WebApi.Entities;

namespace WebApi.Helpers
{
    public class DataContext : DbContext
    {
        protected readonly IConfiguration Configuration;
        public DataContext()
        {
        }
        public DataContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // connect to sql server database
            // connect to sql server database
            // options.UseSqlServer(Configuration.GetConnectionString("WebApiDatabase"));
            // options.UseSqlServer("WebApiDatabase");
            IConfigurationRoot configuration = new ConfigurationBuilder()
             .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
             .AddJsonFile("appsettings.json")
             .Build();
            options.UseSqlServer(configuration.GetConnectionString("WebApiDatabase"));

        }

        public DbSet<User> Users { get; set; }
    }
}