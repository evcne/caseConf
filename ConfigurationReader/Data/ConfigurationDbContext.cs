using Microsoft.EntityFrameworkCore;
using ConfigurationReader.Models;

namespace ConfigurationReader.Data
{
    public class ConfigurationDbContext : DbContext
    {
        public DbSet<ConfigurationEntry> ConfigurationEntries { get; set; }

        /*public ConfigurationDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }*/

        public ConfigurationDbContext(DbContextOptions<ConfigurationDbContext> options)
        : base(options)
        {
        }

    }
}
