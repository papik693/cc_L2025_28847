using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Cdv.Domain.DbContext
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<PeopleDbContext>
    {
        public PeopleDbContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var builder = new DbContextOptionsBuilder<PeopleDbContext>();

            var connectionString = configuration.GetConnectionString("Db");

            builder.UseSqlServer(connectionString);

            return new PeopleDbContext(builder.Options);
        }
    }
}