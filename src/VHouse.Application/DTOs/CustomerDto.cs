using System.ComponentModel.DataAnnotations;

namespace VHouse.Application.DTOs;

public class CreateCustomerDto
{
    [Required]
    [StringLength(200)]
    public string CustomerName { get; set; } = string.Empty;
    
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;
    
    [Phone]
    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string Address { get; set; } = string.Empty;
    
    public bool IsVeganPreferred { get; set; } = true;
}

public class UpdateCustomerDto
{
    [Required]
    [StringLength(200)]
    public string CustomerName { get; set; } = string.Empty;
    
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;
    
    [Phone]
    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string Address { get; set; } = string.Empty;
    
    public bool IsVeganPreferred { get; set; } = true;
    
    public bool IsActive { get; set; } = true;
}

public class CustomerOrderSummaryDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime? LastOrderDate { get; set; }
    public bool IsVeganPreferred { get; set; }
}