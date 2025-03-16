using System.ComponentModel.DataAnnotations;

namespace VHouse.Classes
{

    /// <summary>
    /// Represents a customer in the system.
    /// </summary>
    public class Customer
    {
        /// <summary>
        /// Unique identifier for the customer.
        /// </summary>
        [Key]
        public int CustomerId { get; set; }

        /// <summary>
        /// Full name of the customer.
        /// </summary>
        [Required, StringLength(100)]
        public string FullName { get; set; }

        /// <summary>
        /// Email address of the customer.
        /// </summary>
        [Required, EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// Phone number of the customer.
        /// </summary>
        [Required, Phone]
        public string Phone { get; set; }

        /// <summary>
        /// Address of the customer.
        /// </summary>
        [StringLength(255)]
        public string Address { get; set; }

        /// <summary>
        /// Orders associated with this customer.
        /// </summary>
        public List<Order> Orders { get; set; } = new();
        /// <summary>
        /// Identificar clientes retail.
        /// </summary>
        public bool IsRetail { get; set; }
        
    }
}
