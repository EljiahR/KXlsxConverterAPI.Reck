using KXlsxConverterAPI.Models;
using Microsoft.EntityFrameworkCore;

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
