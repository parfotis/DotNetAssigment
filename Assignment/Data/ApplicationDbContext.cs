using Microsoft.EntityFrameworkCore;
using Assignment.Models;

namespace Assignment.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) {}

        public DbSet<Country> Countries { get; set; }
        public DbSet<IpAddress> IpAddresses { get; set; }

        public DbSet<IpReport> IpReports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IpAddress>().ToTable("IPAddresses");

            //IpAddress
            modelBuilder.Entity<IpAddress>()
            .HasOne(c => c.Country)
            .WithMany(ip => ip.IpAddresses)
            .HasForeignKey(c => c.CountryId);

            //IpReport
            modelBuilder.Entity<IpReport>()
            .HasKey(r => r.CountryName);
        }
    }
}
