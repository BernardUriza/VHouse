using System.ComponentModel.DataAnnotations;
using VHouse.Domain.Enums;

namespace VHouse.Application.DTOs;

public class CreateOrderDto
{
    [Required]
    public int CustomerId { get; set; }
    
    public int? ClientTenantId { get; set; }
    
    [StringLength(1000)]
    public string Notes { get; set; } = string.Empty;
    
    public ICollection<CreateOrderItemDto> OrderItems { get; set; } = new List<CreateOrderItemDto>();
}

public class CreateOrderItemDto
{
    [Required]
    public int ProductId { get; set; }
    
    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal UnitPrice { get; set; }
    
    [StringLength(500)]
    public string Notes { get; set; } = string.Empty;
}

public class AddOrderItemDto
{
    [Required]
    public int ProductId { get; set; }
    
    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal UnitPrice { get; set; }
    
    [StringLength(500)]
    public string Notes { get; set; } = string.Empty;
}

public class OrderSummaryDto
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int PendingOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int CancelledOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
    public DateTime? LastOrderDate { get; set; }
    public int VeganCustomersCount { get; set; }
}