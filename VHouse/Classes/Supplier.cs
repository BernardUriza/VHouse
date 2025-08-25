using System.ComponentModel.DataAnnotations;

namespace VHouse.Classes
{
    /// <summary>
    /// Represents a supplier in the B2B system.
    /// </summary>
    public class Supplier
    {
        /// <summary>
        /// Unique identifier for the supplier.
        /// </summary>
        [Key]
        public int SupplierId { get; set; }

        /// <summary>
        /// Company name of the supplier.
        /// </summary>
        [Required, StringLength(150)]
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// Contact person name at the supplier.
        /// </summary>
        [Required, StringLength(100)]
        public string ContactPerson { get; set; } = string.Empty;

        /// <summary>
        /// Email address of the supplier contact.
        /// </summary>
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Phone number of the supplier.
        /// </summary>
        [Required, Phone]
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// Address of the supplier.
        /// </summary>
        [StringLength(255)]
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Tax identification number.
        /// </summary>
        [StringLength(50)]
        public string TaxId { get; set; } = string.Empty;

        /// <summary>
        /// Payment terms (e.g., "Net 30", "COD").
        /// </summary>
        [StringLength(50)]
        public string PaymentTerms { get; set; } = string.Empty;

        /// <summary>
        /// Indicates if the supplier is currently active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Purchase orders from this supplier.
        /// </summary>
        public List<PurchaseOrder> PurchaseOrders { get; set; } = new();

        /// <summary>
        /// Products supplied by this supplier.
        /// </summary>
        public List<Product> Products { get; set; } = new();
    }
}