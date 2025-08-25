using System.ComponentModel.DataAnnotations;

namespace VHouse.Classes
{
    /// <summary>
    /// Represents a tenant in the multi-tenant distribution system.
    /// </summary>
    public class Tenant
    {
        /// <summary>
        /// Unique identifier for the tenant.
        /// </summary>
        [Key]
        public int TenantId { get; set; }

        /// <summary>
        /// Unique tenant code for identification.
        /// </summary>
        [Required, StringLength(50)]
        public string TenantCode { get; set; } = string.Empty;

        /// <summary>
        /// Display name of the tenant organization.
        /// </summary>
        [Required, StringLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Tenant description.
        /// </summary>
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Primary contact email.
        /// </summary>
        [Required, EmailAddress]
        public string ContactEmail { get; set; } = string.Empty;

        /// <summary>
        /// Primary contact phone.
        /// </summary>
        [Phone]
        public string ContactPhone { get; set; } = string.Empty;

        /// <summary>
        /// Tenant's main address.
        /// </summary>
        [StringLength(500)]
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Tenant configuration as JSON.
        /// </summary>
        public string Configuration { get; set; } = "{}";

        /// <summary>
        /// Whether the tenant is currently active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Date when tenant was created.
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Date when tenant was last updated.
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Distribution centers belonging to this tenant.
        /// </summary>
        public List<DistributionCenter> DistributionCenters { get; set; } = new();

        /// <summary>
        /// Routes managed by this tenant.
        /// </summary>
        public List<DeliveryRoute> DeliveryRoutes { get; set; } = new();
    }

    /// <summary>
    /// Represents a distribution center for multi-location operations.
    /// </summary>
    public class DistributionCenter
    {
        /// <summary>
        /// Unique identifier for the distribution center.
        /// </summary>
        [Key]
        public int DistributionCenterId { get; set; }

        /// <summary>
        /// Foreign key to tenant.
        /// </summary>
        public int TenantId { get; set; }

        /// <summary>
        /// Distribution center code.
        /// </summary>
        [Required, StringLength(50)]
        public string CenterCode { get; set; } = string.Empty;

        /// <summary>
        /// Distribution center name.
        /// </summary>
        [Required, StringLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Full address of the distribution center.
        /// </summary>
        [Required, StringLength(500)]
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// GPS latitude coordinate.
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// GPS longitude coordinate.
        /// </summary>
        public double? Longitude { get; set; }

        /// <summary>
        /// Manager name.
        /// </summary>
        [StringLength(100)]
        public string ManagerName { get; set; } = string.Empty;

        /// <summary>
        /// Contact phone.
        /// </summary>
        [Phone]
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// Contact email.
        /// </summary>
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Whether the center is currently active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Operating capacity (e.g., max orders per day).
        /// </summary>
        public int Capacity { get; set; } = 1000;

        /// <summary>
        /// Navigation property to tenant.
        /// </summary>
        public Tenant? Tenant { get; set; }

        /// <summary>
        /// Warehouses associated with this distribution center.
        /// </summary>
        public List<Warehouse> Warehouses { get; set; } = new();

        /// <summary>
        /// Delivery routes starting from this center.
        /// </summary>
        public List<DeliveryRoute> DeliveryRoutes { get; set; } = new();
    }

    /// <summary>
    /// Represents a delivery route for logistics optimization.
    /// </summary>
    public class DeliveryRoute
    {
        /// <summary>
        /// Unique identifier for the delivery route.
        /// </summary>
        [Key]
        public int DeliveryRouteId { get; set; }

        /// <summary>
        /// Foreign key to tenant.
        /// </summary>
        public int TenantId { get; set; }

        /// <summary>
        /// Foreign key to distribution center.
        /// </summary>
        public int DistributionCenterId { get; set; }

        /// <summary>
        /// Route code for identification.
        /// </summary>
        [Required, StringLength(50)]
        public string RouteCode { get; set; } = string.Empty;

        /// <summary>
        /// Route name.
        /// </summary>
        [Required, StringLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Route description.
        /// </summary>
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Assigned driver name.
        /// </summary>
        [StringLength(100)]
        public string DriverName { get; set; } = string.Empty;

        /// <summary>
        /// Vehicle information.
        /// </summary>
        [StringLength(100)]
        public string Vehicle { get; set; } = string.Empty;

        /// <summary>
        /// Maximum capacity for this route.
        /// </summary>
        public int MaxCapacity { get; set; } = 100;

        /// <summary>
        /// Estimated route duration in minutes.
        /// </summary>
        public int EstimatedDuration { get; set; }

        /// <summary>
        /// Route distance in kilometers.
        /// </summary>
        public decimal Distance { get; set; }

        /// <summary>
        /// Whether the route is currently active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Navigation property to tenant.
        /// </summary>
        public Tenant? Tenant { get; set; }

        /// <summary>
        /// Navigation property to distribution center.
        /// </summary>
        public DistributionCenter? DistributionCenter { get; set; }

        /// <summary>
        /// Deliveries assigned to this route.
        /// </summary>
        public List<Delivery> Deliveries { get; set; } = new();
    }

    /// <summary>
    /// Represents a delivery assignment.
    /// </summary>
    public class Delivery
    {
        /// <summary>
        /// Unique identifier for the delivery.
        /// </summary>
        [Key]
        public int DeliveryId { get; set; }

        /// <summary>
        /// Foreign key to delivery route.
        /// </summary>
        public int? DeliveryRouteId { get; set; }

        /// <summary>
        /// Foreign key to order.
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// Delivery address.
        /// </summary>
        [Required, StringLength(500)]
        public string DeliveryAddress { get; set; } = string.Empty;

        /// <summary>
        /// Recipient name.
        /// </summary>
        [Required, StringLength(200)]
        public string RecipientName { get; set; } = string.Empty;

        /// <summary>
        /// Contact phone for delivery.
        /// </summary>
        [Phone]
        public string ContactPhone { get; set; } = string.Empty;

        /// <summary>
        /// GPS latitude for delivery location.
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// GPS longitude for delivery location.
        /// </summary>
        public double? Longitude { get; set; }

        /// <summary>
        /// Scheduled delivery date.
        /// </summary>
        public DateTime ScheduledDate { get; set; }

        /// <summary>
        /// Actual delivery date.
        /// </summary>
        public DateTime? ActualDeliveryDate { get; set; }

        /// <summary>
        /// Delivery status.
        /// </summary>
        [Required, StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, InTransit, Delivered, Failed

        /// <summary>
        /// Special delivery instructions.
        /// </summary>
        [StringLength(1000)]
        public string Instructions { get; set; } = string.Empty;

        /// <summary>
        /// Delivery notes from driver.
        /// </summary>
        [StringLength(1000)]
        public string Notes { get; set; } = string.Empty;

        /// <summary>
        /// Navigation property to delivery route.
        /// </summary>
        public DeliveryRoute? DeliveryRoute { get; set; }

        /// <summary>
        /// Navigation property to order.
        /// </summary>
        public Order? Order { get; set; }
    }
}