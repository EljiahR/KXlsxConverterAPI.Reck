using KXlsxConverterAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace KXlsxConverterAPI.Data
{
    public class EmployeeContext : IdentityDbContext<EmployeeUser>
    {
        public EmployeeContext(DbContextOptions<EmployeeContext> options) : base(options)
        { 
            Database.EnsureCreated();
        }

        public DbSet<Employee> Employees { get; set; }
    }
}
