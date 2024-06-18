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
    }
}
