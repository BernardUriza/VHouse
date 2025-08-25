using System.ComponentModel.DataAnnotations;

namespace VHouse.Classes
{
    /// <summary>
    /// Represents a warehouse for multi-location inventory management.
    /// </summary>
    public class Warehouse
    {
        /// <summary>
        /// Unique identifier for the warehouse.
        /// </summary>
        [Key]
        public int WarehouseId { get; set; }

        /// <summary>
        /// Name of the warehouse.
        /// </summary>
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Warehouse code (short identifier).
        /// </summary>
        [Required, StringLength(20)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Physical address of the warehouse.
        /// </summary>
        [Required, StringLength(255)]
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// City where the warehouse is located.
        /// </summary>
        [Required, StringLength(100)]
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// State or province.
        /// </summary>
        [StringLength(100)]
        public string State { get; set; } = string.Empty;

        /// <summary>
        /// Postal code.
        /// </summary>
        [StringLength(20)]
        public string PostalCode { get; set; } = string.Empty;

        /// <summary>
        /// Country where the warehouse is located.
        /// </summary>
        [StringLength(100)]
        public string Country { get; set; } = string.Empty;

        /// <summary>
        /// Contact phone number for the warehouse.
        /// </summary>
        [Phone]
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// Email contact for the warehouse.
        /// </summary>
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Warehouse manager name.
        /// </summary>
        [StringLength(100)]
        public string ManagerName { get; set; } = string.Empty;

        /// <summary>
        /// Indicates if the warehouse is currently active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Indicates if this is the main/default warehouse.
        /// </summary>
        public bool IsDefault { get; set; } = false;

        /// <summary>
        /// Date when the warehouse was created.
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Foreign key to distribution center.
        /// </summary>
        public int? DistributionCenterId { get; set; }

        /// <summary>
        /// Navigation property to distribution center.
        /// </summary>
        public DistributionCenter? DistributionCenter { get; set; }

        /// <summary>
        /// Purchase orders received at this warehouse.
        /// </summary>
        public List<PurchaseOrder> PurchaseOrders { get; set; } = new();

        /// <summary>
        /// Inventory items stored at this warehouse.
        /// </summary>
        public List<WarehouseInventory> InventoryItems { get; set; } = new();
    }
}