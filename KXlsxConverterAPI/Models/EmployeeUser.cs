using Microsoft.AspNetCore.Identity;

namespace KXlsxConverterAPI.Models
{
    public class EmployeeUser : IdentityUser
    {
        public override string? UserName { get; set; }
        public override string? Email { get; set; }
    }
}
