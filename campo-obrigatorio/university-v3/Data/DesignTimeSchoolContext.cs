namespace ContosoUniversity.Data
{
    using System.IO;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;

    public class DesignTimeSchoolContext : IDesignTimeDbContextFactory<SchoolContext>
    {
        public SchoolContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

             return new SchoolContext(new DbContextOptionsBuilder<SchoolContext>().UseSqlServer(config.GetConnectionString("DefaultConnection")).Options);
        }
    }
}