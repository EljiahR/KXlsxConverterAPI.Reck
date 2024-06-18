using KXlsxConverterAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.Marshalling;

namespace KXlsxConverterAPI.Data
{
    public class EmployeeContext : DbContext
    {
        public EmployeeContext(DbContextOptions<EmployeeContext> options) : base(options)
        { 
            Database.EnsureCreated();
        }

        public DbSet<Employee> Employees { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Honestly couldn't find anything on setting multiple properties so I apologize if theres a better way
            // Setting nullable fields
            modelBuilder.Entity<Employee>()
                .Property(e => e.PreferredFirstName).IsRequired(false);
            modelBuilder.Entity<Employee>()
                .Property(e => e.Birthday).IsRequired(false);
            modelBuilder.Entity<Employee>()
                .Property(e => e.PositionOverride).IsRequired(false);
            // Setting defaults
            modelBuilder.Entity<Employee>()
                .Property(e => e.PreferredNumberOfBreaks)
                .HasDefaultValue(2);
            modelBuilder.Entity<Employee>()
                .Property(e => e.GetsLunchAsAdult)
                .HasDefaultValue(false);
            modelBuilder.Entity<Employee>()
                .Property(e => e.BathroomOrder)
                .HasDefaultValue(0);
            modelBuilder.Entity<Employee>()
                .Property(e => e.IsACallUp)
                .HasDefaultValue(true);
        }
    }
}
