using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Assignment.Data;
using Assignment.Models;

namespace Assignment.Tests.UnitTests
{
    public abstract class TestWithInMemoryDb
    {
        protected readonly ApplicationDbContext _context;

        protected TestWithInMemoryDb()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new ApplicationDbContext(options);

            ClearDatabase();
            SeedDatabase();
        }

        private void ClearDatabase()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
        }

        protected virtual void SeedDatabase()
        {
            var country = new Country { Name = "Greece", TwoLetterCode = "GR", ThreeLetterCode = "GRC", CreatedAt = DateTime.UtcNow };
            _context.Countries.Add(country);
            
            _context.IpAddresses.AddRange(new List<IpAddress>
            {
                new IpAddress { Ip = "37.6.160.243", Country = country, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new IpAddress { Ip = "37.6.160.244", Country = country, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            });

            _context.SaveChanges();
        }
    }
}