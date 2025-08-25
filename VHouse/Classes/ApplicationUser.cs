using Microsoft.AspNetCore.Identity;

namespace VHouse.Classes
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? CompanyName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        
        // Navigation property to link with Customer if this user is a customer
        public int? CustomerId { get; set; }
        public virtual Customer? Customer { get; set; }
    }
}