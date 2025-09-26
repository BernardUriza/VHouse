using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VHouse.Domain.Entities;

public class PriceList : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public bool IsDefault { get; set; }

    public bool IsActive { get; set; } = true

    public virtual ICollection<PriceListItem> PriceListItems { get; } = new List<PriceListItem>();
    public virtual ICollection<ClientTenantPriceList> ClientTenantPriceLists { get; } = new List<ClientTenantPriceList>();
}

public class PriceListItem : BaseEntity
{
    public int PriceListId { get; set; }
    public virtual PriceList PriceList { get; set; } = null!;

    public int ProductId { get; set; }
    public virtual Product Product { get; set; } = null!;

    [Column(TypeName = "decimal(18,2)")]
    public decimal CustomPrice { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal DiscountPercentage { get; set; } = 0;

    public int MinOrderQuantity { get; set; } = 1;

    public bool IsActive { get; set; } = true;
}

public class ClientTenantPriceList : BaseEntity
{
    public int ClientTenantId { get; set; }
    public virtual ClientTenant ClientTenant { get; set; } = null!;

    public int PriceListId { get; set; }
    public virtual PriceList PriceList { get; set; } = null!;

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}